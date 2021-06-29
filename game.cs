using System;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

class StartScreen : Form {

    static public string name_of_hero = "";
    string[] hero_filenames = {
        "america", "batman", "deadpool", "iron_man", "spiderman", "superman", "thor" };

    public StartScreen() {
        ClientSize = new Size(450, 500);
        BackColor = Color.PowderBlue;

        string[] names_of_buttons = {
            "Captain America", "Batman", "Deadpool", "Iron Man", "Spiderman", "Superman", "Thor"};
        
        int top = 25;
        for (int i = 0 ; i < 7 ; ++i) {
            int j = i;
            Button b = new Button();
            b.Text = "Play with " + names_of_buttons[j];
            b.Width = 400;
            b.Height = 40;
            b.Top = top;
            b.Left = 25;
            b.BackColor = Color.SkyBlue;
            b.Font = new Font("Andale Mono", 15F, FontStyle.Bold);
            b.ForeColor = Color.MidnightBlue;
            b.FlatStyle = FlatStyle.Flat;
            Controls.Add(b);
            top += 65;
            b.Click += new EventHandler((sender, events) => click(hero_filenames[j]));
        }
    }

    void click(string filename) {
        name_of_hero = filename + ".png";
        Application.Run(new MyForm());
        Application.Restart();
    }

}

class MyForm : Form {
     
    bool game_stop;
    bool status_of_shooting; 
    bool up, down;
    int actual_score = 0;
    int previous_score = 0;
    int speed_of_column = 6;
    int speed_of_monster = 7;
    int number_of_monster = 0;
    int speed_of_hero = 8;
    Random random = new Random();
    
    static PictureBox column_1 = new PictureBox();
    static PictureBox column_2 = new PictureBox();
    static PictureBox hero = new PictureBox();
    static PictureBox monster = new PictureBox();
    static IContainer components = new Container();
    static Timer timer = new Timer(components);
    static Label score = new Label();
    static Label hint = new Label();

    string filename(string name) =>
        Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), name);

    Rectangle heroBounds => Rectangle.Inflate(hero.Bounds, -15, -15);
    Rectangle monsterBounds => Rectangle.Inflate(monster.Bounds, -5, -10);

    public MyForm() {

        monster.ImageLocation = filename("thanos.png");
        monster.Location = new Point(1100, 300);
        monster.Size = new Size(70, 120);
        monster.BackColor = Color.Transparent;
        monster.SizeMode = PictureBoxSizeMode.StretchImage;

        column_1.ImageLocation = filename("column.png");
        column_1.Location = new Point(1000, 0);
        column_1.Size = new Size(70, 170);
        column_1.BackColor = Color.Transparent;
        column_1.SizeMode = PictureBoxSizeMode.StretchImage;

        column_2.ImageLocation = filename("column.png");
        column_2.Location = new Point(600, 330);
        column_2.Size = new Size(70, 170);
        column_2.BackColor = Color.Transparent;
        column_2.SizeMode = PictureBoxSizeMode.StretchImage;

        hero.ImageLocation = filename(StartScreen.name_of_hero);
        hero.Location = new Point(100, 100);
        hero.Size = new Size(70, 120);
        hero.BackColor = Color.Transparent;
        hero.SizeMode = PictureBoxSizeMode.StretchImage;

        timer.Enabled = true;
        timer.Interval = 20;
        timer.Tick += new System.EventHandler(Main_timer_event);

        score.AutoSize = true;
        score.Font = new Font("Andale Mono", 25F, FontStyle.Bold);
        score.ForeColor = Color.White;
        score.BackColor = System.Drawing.Color.IndianRed;
        score.Location = new Point(950, 13);
        score.Text = "0";

        hint.AutoSize = true;
        hint.Font = new Font("Andale Mono", 15F, FontStyle.Bold);
        hint.ForeColor = Color.White;
        hint.BackColor = System.Drawing.Color.IndianRed;
        hint.Location = new Point(400, 450);
        hint.Text = "Press SPACE to shoot. Press DOWN or UP to fly.";

        BackgroundImage = Image.FromFile (filename("city.jpg"));
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(1000, 500);
        Controls.Add(score);
        Controls.Add(hint);
        Controls.Add(column_2);
        Controls.Add(column_1);
        Controls.Add(hero);
        Controls.Add(monster);
        Text = "My Hero";
        KeyDown += new KeyEventHandler(Key_is_down);
        KeyUp += new KeyEventHandler(Key_is_up);

  }
  
    void Main_timer_event(object sender, EventArgs events)
    {
        // control score
        score.Text = $"{actual_score}";

        // control hint
        if (game_stop) {
            hint.Text = "Press ENTER to start";
        } else {
            hint.Text = "Press SPACE to shoot. Press DOWN or UP to fly.";
        }

        // control hero
        if (hero.Top > 0 && up == true) {
            hero.Top -= speed_of_hero;
        }
        else if (hero.Top < 500 - hero.Height && down == true) {
            hero.Top += speed_of_hero;
        }

        // control monster
        if (monster.Left < 0) {
            Choose_monster();
        }
        if (heroBounds.IntersectsWith(monsterBounds)) {
            End_of_game();
        }
        monster.Left -= speed_of_monster;

        // control columns
        column_1.Left -= speed_of_column;
        column_2.Left -= speed_of_column;
        if (column_1.Left + 170 < 0) {
            column_1.Left = 1000;
            column_1.Size = new Size(70, random.Next(170, 300));
        }
        if (column_2.Left + 170 < 0) {
            column_2.Left = 1000;
            column_2.Size = new Size(70, random.Next(170, 300));
            column_2.Location = new Point(1000, 500 - column_2.Height);
        }
        if (heroBounds.IntersectsWith(column_1.Bounds) || heroBounds.IntersectsWith(column_2.Bounds)) {
            End_of_game();
        }

        // control bullets
        foreach (Control x in Controls) {

            if ( (string) x.Tag == "bullet") {

                x.Left += 30;

                if (x.Left > 1000) {
                    Bullet_false( (PictureBox) x);
                }

                if (monster.Bounds.IntersectsWith(x.Bounds)) {
                    Bullet_false( (PictureBox) x);
                    actual_score += 1;
                    Choose_monster();
                }
            }
        }

        // controls score
        if (actual_score - previous_score >= 3){
            speed_of_column += 1;
            speed_of_monster += 1;
            previous_score = actual_score;
        }

        // controls hint
        if (actual_score >= 2 && !game_stop){
            hint.Hide();
        }
    }

    void Key_is_down(object sender, KeyEventArgs button) {

        if (button.KeyCode == Keys.Space && status_of_shooting == false) {
            Bullet_true();
            status_of_shooting = true;
        }

        if (button.KeyCode == Keys.Up) {
            up = true;
        }

        else if (button.KeyCode == Keys.Down) {
            down = true;
        }

    }

    void Key_is_up(object sender, KeyEventArgs button) {

        if (button.KeyCode == Keys.Up) {
            up = false;
        }

        else if (button.KeyCode == Keys.Down) {
            down = false;
        }

        status_of_shooting = false;

        if (game_stop == true && button.KeyCode == Keys.Enter) {
            New_game();
        }

    }

    void New_game() {

        up = false; down = false; status_of_shooting = false; game_stop = false;
        actual_score = 0; speed_of_column = 6; speed_of_monster = 7;
        score.Text = $"{actual_score}";
        hint.Text = "Press SPACE to shoot. Press DOWN or UP to fly.";
        column_1.Left = 1000;
        column_2.Left = 650;
        timer.Start();
        Choose_monster();

    }

    void End_of_game() {
        score.Text = $"{actual_score}";
        hint.Text = "Press ENTER to start";
        hint.Show();
        game_stop = true;
        timer.Stop();
    }

    void Bullet_false(PictureBox bullet) { Controls.Remove(bullet); }

    void Bullet_true() {

        PictureBox bullet = new PictureBox();
        bullet.BackColor = Color.Black;
        bullet.Height = 5; bullet.Width = 10;
        bullet.Left = hero.Right; bullet.Top = hero.Top + hero.Height / 2;
        bullet.Tag = "bullet";
        Controls.Add(bullet);

    }

    void Choose_monster() {

        number_of_monster = (number_of_monster + 1) % 3;
        monster.Left = 1100;
        monster.Top = random.Next(500 - monster.Height);

        switch (number_of_monster) {
            case 0:
                monster.ImageLocation = filename("thanos.png");
                break;
            case 1:
                monster.ImageLocation = filename("alian.png");
                break;
            case 2:
                monster.ImageLocation = filename("venom.png");
                break;
        }
    }

}

class Program {
  static void Main() {
    Form form = new StartScreen();
    form.FormBorderStyle = FormBorderStyle.FixedSingle;
    form.MaximizeBox = false;
    Application.Run(form);
  }
}

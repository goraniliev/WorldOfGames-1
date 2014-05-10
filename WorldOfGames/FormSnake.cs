using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WorldOfGames
{
    public partial class FormSnake : Form
    {
        
        // Goleminata na poleto ke bide 20x15
        // so sekoja zemena hrana se zgolemuva za 1 teloto
        // dokolku udri vo zid ili vo nekoj del od teloto zavrsuva igrata
        // intervalot mi se namaluva za 10 posto na sekoe 150 poeni
        // na sekoi 4 sekundi dokolku ne se izede hranata se postavuva na drugo mesto
        
        #region Constructor and Variables

        LinkedList<Point> zmija;
        Point hrana;
        int pravec;  // 0-gore, 1-desno, 2-dole, 3-levo
        Random random;
        int score;
        static int best;
        Color BojaHrana;
        int timerHrana;
        bool GameOver;
        int Nadminato;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FormSnake()
        {
            InitializeComponent();
            random = new Random();

            newGame();
        }
        #endregion

        #region Events

        /// <summary>
        /// Метод кој се повикува преку методот за цртање во picture box полето slika.Invalidate().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slika_Paint(object sender, PaintEventArgs e)
        {
            // crtanje hrana
            if (BojaHrana == Color.Blue)
            {
                e.Graphics.FillRectangle(new SolidBrush(BojaHrana), hrana.X * 20 - 1, hrana.Y * 20 - 1, 22, 22);
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), hrana.X * 20 + 5, hrana.Y * 20 + 5, 10, 10);
            }
            else
            {
                e.Graphics.FillEllipse(new SolidBrush(Color.Black), hrana.X * 20, hrana.Y * 20, 20, 20);
                e.Graphics.FillEllipse(new SolidBrush(BojaHrana), hrana.X * 20 + 2, hrana.Y * 20 + 2, 16, 16);
            }

            // crtanje telo
            foreach (Point p in zmija)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Black), new Rectangle(p.X * 20, p.Y * 20, 20, 20));
                e.Graphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(p.X * 20 + 1, p.Y * 20 + 1, 18, 18));
            }

            // crtanje glava
            Point glava = zmija.First.Value;
            e.Graphics.FillRectangle(new SolidBrush(Color.Green), new Rectangle(glava.X * 20 - 1, glava.Y * 20 - 1, 22, 22));
        }

        /// <summary>
        /// Тајмер за движење на змијата.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            UpDate();
        }

        /// <summary>
        /// Тајмер за местење на храна. Доколку поминат 4 секунди и храната не е изедена се мести на ново место храна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerHrana_Tick(object sender, EventArgs e)
        {
            if (timerHrana-- == 0)
                namestiHrana();
        }

        /// <summary>
        /// Евент за нова игра, доколку е во тек играта се поставува прашање дали да се прекине тековната и да се започне нова игра.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (!GameOver)
            {
                if (MessageBox.Show("New Game", "Really?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    newGame();
            }
            else
                newGame();
        }

        /// <summary>
        /// Бидејќи имам едно копче и еден picture box затоа го користам овој евент а не обичниот за keypress бидејќи со тој евентите ќе ги превзема копчето.
        /// Тука го забележав тој проблем http://stackoverflow.com/questions/818092/key-down-event-affected-by-buttons .
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (GameOver)
                return base.ProcessCmdKey(ref msg, keyData);

            int dvizenje = -1;
            Point glava = zmija.ElementAt(0);
            int x = glava.X, y = glava.Y;
           
            if (keyData == Keys.Up)
            {
                y--;
                dvizenje = 0;
            }
            else if (keyData == Keys.Right)
            {
                x++;
                dvizenje = 1;
            }
            else if (keyData == Keys.Down)
            {
                y++;
                dvizenje = 2;
            }
            else if (keyData == Keys.Left)
            {
                x--;
                dvizenje = 3;
            }

            Point p = new Point(x, y);

            // dokolku e validno kopce pritisnato i dokolku ne nastanal sudir so teloto ili zidovite1 togas se menja pravecot
            if (dvizenje != -1 && validno(p))
                pravec = dvizenje;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Logic

        /// <summary>
        /// Метод за почеток на нова игра, се ресетираат вредностите.
        /// </summary>
        private void newGame()
        {
            zmija = new LinkedList<Point>();

            GameOver = false;
            Nadminato = 150;
            pravec = random.Next(4);
            zmija.AddFirst(new Point(10, 7));
            namestiHrana();
            UpDate();
            Timer.Interval = 150;
            Timer.Start();
            score = 0;
        }

        /// <summary>
        /// Метод за движење на змијата.
        /// </summary>
        private void UpDate()
        {
            Point n = zmija.First.Value;
            bool izedena = (n == hrana) ? true : false;

            if (pravec == 0)        // gore
                n.Y--;

            else if (pravec == 1)   // desno
                n.X++;
             
            else if (pravec == 2)    // dole
                n.Y++;
           
            else if (pravec == 3)   // levo
                n.X--;

            if (!validno(n))
            {
                gameOver();
                return;
            }

            zmija.AddFirst(n);
            // dokolku ja izela hranata ne treba da go brise posledniot zs neli se zgolemuva za eden
            if (!izedena) zmija.RemoveLast();
            else
            {
                int sc = 0;
                if (BojaHrana == Color.Red)
                    sc = 5;
                else if (BojaHrana == Color.Yellow)
                    sc = 10;
                else if (BojaHrana == Color.Orange)
                    sc = 15;
                else if (BojaHrana == Color.Brown)
                    sc = 20;
                else       // a dokolku e sina togas se brise pola telo na zmijata
                {
                    int m = zmija.Count;
                    for (int i = 0; i < (m+1) / 2; i++)
                        zmija.RemoveLast();
                }
                
                score += sc;
                if (score > Nadminato)
                {
                    Timer.Interval -= (Timer.Interval / 10);  // 10 posto
                    Nadminato += 150;
                }

                best = (score > best) ? score : best;

                namestiHrana();
            }

            // crtanje
            LbScore.Text = score.ToString();
            LbBest.Text = best.ToString();
            Slika.Invalidate();
        }

        /// <summary>
        /// Метод за местење рандом храната.
        /// </summary>
        private void namestiHrana()
        {
            Point point = new Point( random.Next(20), random.Next(15));

            if (zmija.Contains(point))
            {
                namestiHrana();
                return;
            }

            int rand = random.Next(145);

            if (rand < 50)
                BojaHrana = Color.Red;
            else if (rand < 90)
                BojaHrana = Color.Yellow;
            else if (rand < 120)
                BojaHrana = Color.Orange;
            else if (rand < 140)
                BojaHrana = Color.Brown;
            else
                BojaHrana = Color.Blue;

            hrana = point;

            TimerHrana.Start();
            timerHrana = 4;
        }

        /// <summary>
        /// Метод за проверка дали е валидно полето за движење (дали се судира во телото или со ѕидовите).
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool validno(Point p)
        {
            if (zmija.Contains(p))
                return false;

            if (p.X == -1 || p.X == 20)
                return false;

            if (p.Y == -1 || p.Y == 15)
                return false;

            return true;
        }

        /// <summary>
        /// Крај на играта.
        /// </summary>
        private void gameOver()
        {
            Timer.Stop();
            TimerHrana.Stop();
            GameOver = true;

            MessageBox.Show("Score: "+score.ToString(), "Game Over", MessageBoxButtons.OK);
        }
        #endregion
    }
}

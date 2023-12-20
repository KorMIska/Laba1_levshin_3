using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Laba1_levshin
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {


            Graphics g = pictureBox1.CreateGraphics();
            g.Clear(BackColor);
            DrawPole(g);

            Model mod = new Model(comands);

            List<Itm> m = mod.Rabot();


            trackBar1.Maximum = m[m.Count - 1].time / (750/ WIDTH) + 1;


            for (int i = 0; i < m.Count; i++)
            {
                int j = trackBar1.Value * (750 / WIDTH);
                switch (m[i].type)
                {
                    case 0: ZeroComand (g, m[i].time - j, m[i].nomer + 1);         break;
                    case 1: OneComand  (g, m[i].time - j, m[i].nomer + 1);         break;
                    case 2: TwoComand  (g, m[i].time - j, m[i].nomer + 1, m[i].T); break;
                    case 3: ThreeComand(g, m[i].time - j, m[i].nomer + 1, m[i].T); break;
                    case 4: FourComand (g, m[i].time - j, m[i].nomer + 1);         break;
                }
            }
        }


        public List<Comand> comands = new List<Comand>();

        #region consts
        public const int HEIGHT = 40; // отрисовка
        public const int WIDTH = 50; // отрисовка
        public const int LOCAL_ONE_LINE = 125; // отрисовка
        public const int LOCAL_TWO_LINE = LOCAL_ONE_LINE + HEIGHT * 2 + HEIGHT / 2;// отрисовка
        public const int Tsh = 2;
        public const int Fo = 3;
        #endregion

        #region модель
        // 0 - черта 1 - декодировка 2 - вычесление 3 - упровление 4 - кеш
        public struct Comand
        {
            public int t;
            public bool kh;
            public int type;   // 0 - черта 1 - декодировка 2 - вычесление 3 - упровление 4 - кеш
            public int nomer;
            public int time;

            public Comand(int t, bool kh, int type)
            {
                this.t = t;
                this.kh = kh;
                this.type = type;
                nomer = 0;
                time = 0;
            }
            public Comand(int t, bool kh)
            {
                this.t = t;
                this.kh = kh;
                this.type = 0;
                nomer = 0;
                time = 0;
            }
        }

        public struct Itm
        {
            public int nomer;
            public int time;
            public int type;
            public int T;
            public Itm(int nomer, int time, int type, int T)
            {
                this.nomer = nomer;
                this.time = time;
                this.type = type;
                this.T = T;
            }
        }

        public partial class K1
        {
            public int timeStop;
            public int zadacha;

            public K1()
            {
                timeStop = 0;
                zadacha = 0;
            }
        }

        public partial class KK
        {
            public int timeStop;

            public KK()
            {
                timeStop = 0;
            }
        }

        public partial class SH
        {
            public int timeStop;
        }

        public partial class Model
        {
            KK kk = new KK();
            K1 k1 = new K1();
            SH sh = new SH();

            List<Itm> queueKK = new List<Itm>();
            List<Itm> queueK1 = new List<Itm>();

            List<Comand> cs;

            public Model(List<Comand> cs) { this.cs = cs; }

            public List<Itm> Rabot()
            {
                List<Itm> DrawCom = new List<Itm>();
                int time = 0;

                int countCom = 0;

                //Один цыкл == Один такт

                while (true)
                {
                    // Если конвеир свободен и нет заявок от кэш контролера
                    // Обрабатывается новая команда
                    if (k1.timeStop <= 0 & queueK1.Count == 0)
                    {
                        // countCom - номер команды
                        // проверка того чтобы countCom не был больше количества команд
                        // Что означает что все команды обработаны
                        if (countCom == cs.Count)
                        {
                            //Доп проверка того что выполнены команды из КК
                            if (queueK1.Count == 0 & queueKK.Count == 0)
                                return DrawCom;
                        }
                        else
                        {
                            // Выполняется Декодировка

                            if (cs[countCom].kh)  // Данные есть в кэше
                            {

                                //Команда для отрисовки 

                                //
                                queueK1.Add(new Itm(countCom, time, 1, cs[countCom].t));
                                queueK1.Add( new Itm(countCom, time, cs[countCom].type, cs[countCom].t));


                            }
                            else // Кэш промах
                            {

                                // Отпрака запроса в КК
                                queueKK.Add(new Itm(countCom, time, cs[countCom].type, cs[countCom].t));

                                //Отрисовка Команды 
                                DrawCom.Add(new Itm(countCom, time, 0, cs[countCom].t));

                                countCom++;
                                // continue чтобы не защитало такт
                                continue;
                            }
                            countCom++;

                        }
                    }

                    // К1 свободен и есть заявка на работу
                    if (k1.timeStop <= 0 & queueK1.Count != 0)
                    {
                        // type == уровление устройством 
                        if (queueK1[0].type == 3)
                        {
                            // КК должен быть свободен 
                            // Иначе Должен ждать 
                            if (sh.timeStop <= 0)
                            {
                                Itm cur = queueK1[0];

                                k1.timeStop = Tsh * cur.T;
                                sh.timeStop = Tsh * cur.T;

                                cur.time = time;

                                DrawCom.Add(cur);
                                queueK1.RemoveAt(0);
                            }
                        }
                        else
                        {
                            if (queueK1[0].type == 2)
                            {

                                Itm cur = queueK1[0];
                                cur.time = time;
                                cur.type = 2;
                                DrawCom.Add(cur);
                                queueK1.RemoveAt(0);
                                k1.timeStop = 1 * cur.T;
                            }
                            else
                            {
                                if (queueK1[0].type == 1)
                                {
                                    k1.timeStop = 1;
                                    Itm cur = queueK1[0];
                                    cur.time = time;
                                    cur.type = 1;
                                    DrawCom.Add(cur);
                                    DrawCom.Add(new Itm(cur.nomer, time + 1, 0, 1));
                                    queueK1.RemoveAt(0);
                                }
                            }
                        }
                    }
                    // КК свободен и есть запрос
                    if (kk.timeStop <= 0 & queueKK.Count != 0 & sh.timeStop<=0)
                    {

                        kk.timeStop = Tsh * Fo;
                        sh.timeStop = Tsh * Fo;

                        Itm cur = queueKK[0];

                        cur.time = time;

                        DrawCom.Add(new Itm(queueKK[0].nomer, time, 4, queueKK[0].T));

                    }


                    // КК отправляет ответ о завершении кэширования

                    if (kk.timeStop - 1 == 0)
                    {
                        Itm cur = queueKK[0];

                        cur.time = time;

                        queueK1.Add(new Itm(cur.nomer, time, 1, 1));
                        queueK1.Add(cur);

                        queueKK.RemoveAt(0);
                    }

                    // обнуление задачи
                    if (k1.timeStop - 1 == 0)
                    {
                        k1.zadacha = 0;
                    }

                    // Условный такт
                    kk.timeStop--;
                    k1.timeStop--;
                    sh.timeStop--;

                    time++;

                }

            }
        }

        #endregion

        #region "отрисовка"

        public int ooe = 0;

        private void ZeroComand(Graphics g, int i, int n)
        {
            g.DrawLine(new Pen(Color.Blue, 4), new Point(i * WIDTH, LOCAL_ONE_LINE), new Point(i * WIDTH, LOCAL_ONE_LINE - HEIGHT - 20 ));
            var rect = new Rectangle(i * WIDTH + 6, LOCAL_ONE_LINE - HEIGHT - 20 - ooe*11, 15, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            ooe++;
        }
        private void OneComand(Graphics g, int i, int n)
        {
            g.DrawPolygon(
                new Pen(Color.White, 3),
                new Point[]{
                new Point(i *     WIDTH, LOCAL_ONE_LINE),
                new Point(i *     WIDTH, LOCAL_ONE_LINE - HEIGHT),
                new Point((i+1) * WIDTH, LOCAL_ONE_LINE - HEIGHT),
                new Point((i+1) * WIDTH, LOCAL_ONE_LINE)});

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_ONE_LINE - HEIGHT + 6, 15, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            ooe = 0;

        }
        private void TwoComand(Graphics g, int i, int n, int T)
        {
            g.DrawPolygon(
                new Pen(Color.White, 3),
                new Point[]{
                new Point(i * WIDTH,       LOCAL_ONE_LINE),
                new Point(i * WIDTH,       LOCAL_ONE_LINE + HEIGHT),
                new Point((i+T) * WIDTH,   LOCAL_ONE_LINE + HEIGHT),
                new Point((i+T) * WIDTH,   LOCAL_ONE_LINE)
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_ONE_LINE + 12, 15, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            ooe = 0;
        }
        private void ThreeComand(Graphics g, int i, int n, int T)
        {
            Brush b = new SolidBrush(Color.FromArgb(255, 255, 255));

            g.FillPolygon(b,
                new Point[]{
                new Point(i * WIDTH,     LOCAL_ONE_LINE),
                new Point(i * WIDTH,     LOCAL_ONE_LINE + HEIGHT),
                new Point((i+Tsh*T) * WIDTH, LOCAL_ONE_LINE + HEIGHT),
                new Point((i+Tsh*T) * WIDTH, LOCAL_ONE_LINE)
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_ONE_LINE + 12, 14, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.Black,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            ooe = 0;
        }
        private void FourComand(Graphics g, int i, int n)
        {
            Brush b = new SolidBrush(Color.FromArgb(255, 255, 255));

            g.FillPolygon(b,
                new Point[]{
                new Point(i * WIDTH, LOCAL_TWO_LINE),
                new Point(i * WIDTH, LOCAL_TWO_LINE - HEIGHT),
                new Point((i+Tsh*Fo) * WIDTH, LOCAL_TWO_LINE - HEIGHT),
                new Point((i+Tsh*Fo) * WIDTH,LOCAL_TWO_LINE)
            });

            var rect = new Rectangle(i * WIDTH + 6, LOCAL_TWO_LINE - HEIGHT + 6, 15, 15);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextRenderer.DrawText(g, n.ToString(), new Font("Arial", 10), rect, Color.Black,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            ooe = 0;
        }

        private void DrawPole(Graphics g)
        {

            Pen whit = new Pen(Color.White, 2);


            Point line_n1 = new Point(0, LOCAL_ONE_LINE);
            Point line_e1 = new Point(2000, LOCAL_ONE_LINE);

            Point line_n2 = new Point(0, LOCAL_TWO_LINE);
            Point line_e2 = new Point(2000, LOCAL_TWO_LINE);

            g.DrawLine(new Pen(Color.White, 4), line_n1, line_e1);
            g.DrawLine(new Pen(Color.White, 4), line_n2, line_e2);

            Point[] chtrih1 = new Point[100];
            Point[] chtrih2 = new Point[100];

            for (int i = 0; i < chtrih1.Length; i++)
            {
                chtrih1[i] = new Point(i * WIDTH, LOCAL_ONE_LINE - 5);
            }


            for (int i = 0; i < chtrih2.Length; i++)
            {
                chtrih2[i] = new Point(i * WIDTH, LOCAL_ONE_LINE + 5);
            }

            for (int i = 0; i < chtrih1.Length; i++)
            {
                g.DrawLine(whit, chtrih1[i], chtrih2[i]);
                g.DrawLine(whit, new Point(chtrih1[i].X, chtrih1[i].Y + 100), new Point(chtrih2[i].X, chtrih2[i].Y + 100));
            }
        }

        private void pictureBox1_MouseMove_1(object sender, MouseEventArgs e)
        {
        }

        #endregion

        #region интерфейс
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            // щаблон 
            comands = new List<Comand>
           {
                new Comand(2, false, 3),
                new Comand(2, false, 2),
                new Comand(2, true, 2),
                new Comand(2, true, 3),
                new Comand(1,false, 3),
                new Comand(1,false, 2),
                new Comand(1,false, 2),
                new Comand(2,true, 3),
                new Comand(1,true, 2),
                new Comand(1,false, 3),
                new Comand(1,true, 3),
                new Comand(1,false, 2),
                new Comand(1,true, 2)
           };
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
          
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void button6_Click(object sender, EventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        #endregion

        public void Text_UPdata()
        {
            richTextBox1.Text = "";
            for (int i = 0; i < comands.Count; i++)
            {
                richTextBox1.Text += $"{i} {comands[i].t}t({(comands[i].kh?"КЭШ": "НЕКЭШ")},{(comands[i].type == 2 ?"--": "УО")})\n";
            }
        }

        public int n = 0;
        private void button1_Click_1(object sender, EventArgs e)
        {
            var cur = comands[comands.Count-1];
            cur.t = 2;
            comands[comands.Count - 1] = cur;
            Text_UPdata();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            comands.Add(new Comand(1, true, 2));
            Text_UPdata();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            comands.Add(new Comand(1, true, 3));
            Text_UPdata();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            comands.Add(new Comand(1, false, 2));
            Text_UPdata();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comands.Add(new Comand(1, false, 3));
            Text_UPdata();
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            comands.RemoveAt(comands.Count-1);
            Text_UPdata();
        }
    }
}

  


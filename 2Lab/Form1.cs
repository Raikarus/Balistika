using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.Platform.Windows;
using Tao.FreeGlut;

namespace _2Lab
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            energy.InitializeContexts();
            graph.InitializeContexts();
            AnT.InitializeContexts();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // инициализация библиотеки glut 
            Glut.glutInit();
            // инициализация режима экрана 
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE);

            // установка цвета очистки экрана (RGBA) 
            Gl.glClearColor(255, 255, 255, 1);

            // установка порта вывода 
            Gl.glViewport(0, 0, AnT.Width, AnT.Height);

            // активация проекционной матрицы 
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            // очистка матрицы 
            Gl.glLoadIdentity();

            // определение параметров настройки проекции в зависимости от размеров сторон элемента AnT. 
            if ((double)AnT.Width <= (double)AnT.Height)
            {
                ScreenW = 30.0;
                ScreenH = 30.0 * (double)AnT.Height / (double)AnT.Width;
                Glu.gluOrtho2D(0.0, ScreenW, 0.0, ScreenH);
            }
            else
            {
                ScreenW = 30.0 * (double)AnT.Width / (double)AnT.Height;
                ScreenH = 30.0;
                Glu.gluOrtho2D(0.0, 30.0 * (double)AnT.Width / (double)AnT.Height, 0.0, 30.0);
            }

            // сохранение коэффициентов, которые нам необходимы для перевода координат указателя в оконной системе в координаты, 
            // принятые в нашей OpenGL сцене 
            devX = (double)ScreenW / (double)AnT.Width;
            devY = (double)ScreenH / (double)AnT.Height;

            // установка объектно-видовой матрицы 
            Gl.glMatrixMode(Gl.GL_MODELVIEW);


            // старт счетчика, отвечающего за вызов функции визуализации сцены 
            PointInGrap.Start();

        }


        // размеры окна 
        double ScreenW, ScreenH;

        // отношения сторон окна визуализации 
        // для корректного перевода координат мыши в координаты, 
        // принятые в программе 

        private double devX;
        private double devY;

        // массив, который будет хранить значения x,y точек графика 
        private double[,] GrapValuesArray;
        // количество элементов в массиве 
        private int elements_count = 0;

        // флаг, означающий, что массив с значениями координат графика пока еще не заполнен 
        private bool not_calculate = true;

        // номер ячейки массива, из которой будут взяты координаты для красной точки 
        // для визуализации текущего кадра 
        private int pointPosition = 0;

        private double H, V0, g, angle, total_time;

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            pointPosition++;
            // функция визуализации
            AnT.MakeCurrent();
            Draw();


            energy.MakeCurrent();
            DrawEnergy();
        }

        private void reBuild(object sender, EventArgs e)
        {
            try
            {
                xMax = Int32.Parse(textBox2.Text);
                yMax = Int32.Parse(textBox4.Text);
                H = double.Parse(textBox5.Text);
                g = double.Parse(textBox6.Text);
                angle = double.Parse(textBox7.Text);
                V0 = double.Parse(textBox8.Text);
                not_calculate = true;
                elements_count = 0;
                pointPosition = 0;
                DrawDiagram();
                Draw();
                textBox9.Text = $"{GrapValuesArray[elements_count-1,1]}";
            }
            catch(Exception er)
            {
                MessageBox.Show(er.ToString());
            }
        }

        private void DrawDiagram()
        {
            Gl.glViewport(0, 0, AnT.Width, AnT.Height);

            // активация проекционной матрицы 
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            // очистка матрицы 
            Gl.glLoadIdentity();
            ScreenW = (xMax+0.5) * (double)AnT.Width / (double)AnT.Height;
            ScreenH = (yMax+0.5) * (double)AnT.Height / (double)AnT.Width;
            Glu.gluOrtho2D(0.0, ScreenW, 0.0, ScreenH);
            // сохранение коэффициентов, которые нам необходимы для перевода координат указателя в оконной системе в координаты, 
            // принятые в нашей OpenGL сцене 
            devX = (double)ScreenW / (double)AnT.Width;
            devY = (double)ScreenH / (double)AnT.Height;

            // установка объектно-видовой матрицы 
            Gl.glMatrixMode(Gl.GL_MODELVIEW);


            // проверка флага, сигнализирующего о том, что координаты графика вычислены 
            if (not_calculate)
            {
                // если нет, то вызываем функцию вычисления координат графика 
                functionCalculation();
            }

            Gl.glBegin(Gl.GL_LINE_STRIP);
            Gl.glVertex2d(0, 0);
            Gl.glVertex2d(xMax*2, 0);
            Gl.glEnd();

            // стартуем отрисовку в режиме визуализации точек 
            // объединяемых в линии (GL_LINE_STRIP) 
            Gl.glBegin(Gl.GL_POINTS);

            Gl.glColor3ub(colors[0].R, colors[0].G, colors[0].B);
            // рисуем начальную точку 
            if (GrapValuesArray[0, 1] >= 0 && GrapValuesArray[0, 1] <= yMax)
                Gl.glVertex2d(GrapValuesArray[0, 0], GrapValuesArray[0, 1]);

            // проходим по массиву с координатами вычисленных точек 
            for (int ax = 1; ax < pointPosition; ax += 2)
            {
                // передаем в OpenGL информацию о вершине, участвующей в построении линий
                if(GrapValuesArray[ax, 1] >= 0 && GrapValuesArray[ax, 1] <= yMax)
                    Gl.glVertex2d(GrapValuesArray[ax, 0], GrapValuesArray[ax, 1]);
            }

            // завершаем режим рисования 
            Gl.glEnd();
            // устанавливаем размер точек, равный 5 пикселям
            Gl.glPointSize(15);
            // устанавливаем текущим цветом - красный цвет 
            Gl.glColor3ub(colors[3].R, colors[3].G, colors[3].B);
            // активируем режим вывода точек (GL_POINTS) 
            Gl.glBegin(Gl.GL_POINTS);
            // выводим красную точку, используя ту ячейку массива, до которой мы дошли (вычисляется в функции обработчике событий таймера) 
            if (pointPosition != elements_count - 1)
                Gl.glVertex2d(GrapValuesArray[pointPosition, 0], GrapValuesArray[pointPosition, 1]);
            else
            {
                Gl.glVertex2d(GrapValuesArray[pointPosition, 0], GrapValuesArray[pointPosition, 1]);
                //Gl.glVertex2d(GrapValuesArray[pointPosition, 0], 0);
                pointPosition = 0;
            }

            // завершаем режим рисования 
            Gl.glEnd();
            // устанавливаем размер точек равный единице
            Gl.glPointSize(5);

        }

        private int xMax,  yMax;


        private Color[] colors = new Color[6];

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
                 if (comboBox3.SelectedText == "График") colors[0] = colorDialog1.Color;
            else if (comboBox3.SelectedText == "Оси") colors[1] = colorDialog1.Color;
            else if (comboBox3.SelectedText == "Сетка точек") colors[2] = colorDialog1.Color;
            else if (comboBox3.SelectedText == "Точка") colors[3] = colorDialog1.Color;
            else if (comboBox3.SelectedText == "Линии курсора") colors[4] = colorDialog1.Color;
            else if (comboBox3.SelectedText == "Подпись координат") colors[5] = colorDialog1.Color;
            DrawDiagram();
            Draw();
        }

        private void Draw()
        {

            // очистка буфера цвета и буфера глубины 
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            // очищение текущей матрицы 
            Gl.glLoadIdentity();

            // помещаем состояние матрицы в стек матриц 
            Gl.glPushMatrix();

            // выполняем перемещение в пространстве по осям X и Y 
          //  Gl.glTranslated(-xMin, -yMin, 0);




            // вызываем функцию рисования графика 
            DrawDiagram();

            // возвращаем матрицу из стека 
            Gl.glPopMatrix();
           

            // дожидаемся завершения визуализации кадра 
            Gl.glFlush();

            // сигнал для обновление элемента реализующего визуализацию. 
            AnT.Invalidate();

        }


        private double Vy;
        private void functionCalculation()
        {

            // определение локальных переменных X и Y 
            double x = 0, y = 0;

            // инициализация массива, который будет хранить значение 300 точек,  
            // из которых будет состоять график 
            GrapValuesArray = new double[99999, 3];

            // счетчик элементов массива 
            elements_count = 0;
            double diskr = Math.Sqrt(V0*Math.Sin(angle*Math.PI/180) * V0* Math.Sin(angle * Math.PI / 180) + 2 * g * H);
            double t1 = (-V0* Math.Sin(angle * Math.PI / 180) + diskr) / (-g);
            double t2 = (-V0* Math.Sin(angle * Math.PI / 180) - diskr) / (-g);
            total_time = (t1 > 0 && t2 > 0) ? Math.Min(t1, t2) : Math.Max(t1, t2);
            //total_time = ((((-V0 + Math.Sqrt(V0 * V0 - 2 * g * H)) / 2 / H) > 0) && (((-V0 - Math.Sqrt(V0 * V0 - 2 * g * H)) / 2 / H) > 0)) ? Math.Min((-V0 + Math.Sqrt(V0 * V0 - 2 * g * H)) / 2 / H, (-V0 - Math.Sqrt(V0 * V0 - 2 * g * H)) / 2 / H) : Math.Max((-V0 + Math.Sqrt(V0 * V0 - 2 * g * H)) / 2 / H, (-V0 - Math.Sqrt(V0 * V0 - 2 * g * H)) / 2 / H);
            // вычисления всех значений y для x, принадлежащего промежутку от -15 до 15 с шагом в 0.01f 
            for (double t = 0; t <= total_time; t += 0.05f)
            {
                // вычисление y для текущего x 
                // по формуле y = (double)Math.Sin(x)*3 + 1; 
                // эта строка задает формулу, описывающую график функции для нашего уравнения y = f(x). 
                y = (double)(H + V0 * t * Math.Sin(angle/180*Math.PI) - g*t*t/2);
                x = (double)(V0 * Math.Cos(angle / 180 * Math.PI)*t);
                Vy = V0 * Math.Sin(angle / 180 * Math.PI) - g*t;
                
                
                //if (y < 0) break;
                // запись координаты x 
                GrapValuesArray[elements_count, 0] = x+2;
                // запись координаты y 
                if (t == total_time) y = 0;
                GrapValuesArray[elements_count, 1] = y;
                // подсчет элементов 
                GrapValuesArray[elements_count, 2] = (double)(V0 * V0 * Math.Cos(angle / 180 * Math.PI) * Math.Cos(angle / 180 * Math.PI) + Vy*Vy);

                elements_count++;
            }
            // изменяем флаг, сигнализировавший о том, что координаты графика не вычислены 
            not_calculate = false;

        }

        private void DrawEnergy()
        {
            // очистка буфера цвета и буфера глубины 
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
           
            Gl.glViewport(0, 0, energy.Width, energy.Height);
            // активация проекционной матрицы 
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            // очистка матрицы 
            Gl.glLoadIdentity();
            ScreenW = (double)energy.Width / (double)energy.Height;
            ScreenH = (double)energy.Height / (double)energy.Width;
            Glu.gluOrtho2D(0.0, ScreenW, 0.0, ScreenH);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);

            

            

            // помещаем состояние матрицы в стек матриц 
            Gl.glPushMatrix();
            //Gl.glRectd(0, 0, 1, 4);

            double Ep = double.Parse(textBox1.Text)*g*GrapValuesArray[pointPosition,1];
            
            double Ek = double.Parse(textBox1.Text) * GrapValuesArray[pointPosition,2] / 2;
            textBox3.Text = $"{Ep} ; {GrapValuesArray[pointPosition, 1]}";
            Gl.glColor3d(1, 0, 0);
            Gl.glRectd(0.05, 0, 0.15, 2*Ep/(Ep+Ek));
            Gl.glRectd(0.25, 0, 0.35, 2*Ek/(Ep+Ek));
            //Gl.glBegin(Gl.GL_QUADS);
            //Gl.glColor3d(1, 0, 0);
            //Gl.glVertex2d(0, 0);
            //Gl.glVertex2d(0, 1);
            //Gl.glVertex2d(1, 1);
            //Gl.glVertex2d(1, 0);
            // возвращаем матрицу из стека 
            Gl.glPopMatrix();


            // дожидаемся завершения визуализации кадра 
            Gl.glFlush();

            energy.Invalidate();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace Lab4


{
    public partial class Form1 : Form
    {
        Image<Gray, byte> inputImage = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnReview_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = openFileDialog1.ShowDialog();

                if (result == DialogResult.OK)
                {
                    inputImage = new Image<Gray, byte>(openFileDialog1.FileName);
                    tbPath.Text = openFileDialog1.FileName;
                    btnCalculate_Click(this, null);
                }
                else
                    MessageBox.Show("Файл не выбран", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            //Лапласиан
            int[,] mask = new int[,] {  { -1, -1, -1 },
                                          { -1, 8, -1 },
                                          { -1, -1, -1 } };
            inputImage = new Image<Gray, byte>(tbPath.Text); //Основное изображение.
            
            imageList1.Images.Add(inputImage.Bitmap);
            listView1.Items.Add("Исходное изображение", 0);

            Image<Gray, byte> image = new Image<Gray, byte>(tbPath.Text);
            int imageWidth = inputImage.Cols, imageHeight = inputImage.Rows;
            double[,] grayA = new double[imageWidth, imageHeight];

            for (int i = 1; i < (imageWidth - 1); i++)
                for (int j = 1; j < (imageHeight - 1); j++)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                        {
                            Gray color = image[j + y, i + x];
                            grayA[i, j] += color.Intensity * mask[y + 1, x + 1];
                        }

            for (int x = 0; x < imageWidth - 1; x++) //Удаление максимума и минимума
                for (int y = 0; y < imageHeight - 1; y++)
                {
                    image[y, x] = new Gray(grayA[x, y]);
                }

            //Градационная коррекция
            Image<Gray, byte> outputImage = image.Clone();
            double min = double.MaxValue, max = double.MinValue;

            for (int i = 0; i < imageWidth - 1; i++) //минимальный и максимальный цвет массива элементов
                for (int j = 0; j < imageHeight - 1; j++)
                    if (min > grayA[i, j]) min = grayA[i, j];

            for (int x = 0; x < imageWidth - 1; x++) //Удаление максимума и минимума
                for (int y = 0; y < imageHeight - 1; y++)
                    grayA[x, y] = grayA[x, y] - min;

            for (int i = 0; i < imageWidth - 1; i++) //минимальный и максимальный цвет массива элементов
                for (int j = 0; j < imageHeight - 1; j++)
                    if (max < grayA[i, j]) max = grayA[i, j];

            for (int x = 0; x < imageWidth - 1; x++) //Удаление максимума и минимума
                for (int y = 0; y < imageHeight - 1; y++)
                {
                    grayA[x, y] = 255 * grayA[x, y] / max;
                    outputImage[y, x] = new Gray(grayA[x, y]);
                }
            imageList1.Images.Add(outputImage.Bitmap);
            listView1.Items.Add("Лапласиан с градационной коррекцией", 1);

            //Сложение исходного изображения и Лапласиана
            Image<Gray, byte> imageInputOrLaplasian = inputImage + image;
            imageList1.Images.Add(imageInputOrLaplasian.Bitmap);
            listView1.Items.Add("Сложение исходного изображения и Лапласиана", 2);


            // Собель
            int[,] maskS1 = new int[,] {  { -1, -2, -1 },
                                          { 0, 0, 0 },
                                          { 1, 2, 1 } };

            int[,] maskS2 = new int[,] {  { -1, 0, 1 },
                                          { -2, 0, 2 },
                                          { -1, 0, 1 } };

            Image<Gray, byte> imageS1 = inputImage.Clone();
            Image<Gray, byte> imageS2 = inputImage.Clone();

            double[,] imageSobel1 = new double[imageWidth, imageHeight];
            double[,] imageSobel2 = new double[imageWidth, imageHeight];

            for (int i = 1; i < (imageWidth - 1); i++)
                for (int j = 1; j < (imageHeight - 1); j++)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                        {
                            Gray color = imageS1[j + y, i + x];
                            imageSobel1[i, j] += color.Intensity * maskS1[y + 1, x + 1];
                        }
            for (int x = 0; x < imageWidth - 1; x++) //Удаление максимума и минимума
                for (int y = 0; y < imageHeight - 1; y++)
                {
                    imageS1[y, x] = new Gray(imageSobel1[x, y]);
                }

            for (int i = 1; i < (imageWidth - 1); i++)
                for (int j = 1; j < (imageHeight - 1); j++)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                        {
                            Gray color = imageS2[j + y, i + x];
                            imageSobel2[i, j] += color.Intensity * maskS2[y + 1, x + 1];
                        }
            for (int x = 0; x < imageWidth - 1; x++) //Удаление максимума и минимума
                for (int y = 0; y < imageHeight - 1; y++)
                {
                    imageS2[y, x] = new Gray(imageSobel2[x, y]);
                }

            Image<Gray, byte> viewImage = new Image<Gray, byte>(imageWidth, imageHeight);
            for (int x = 0; x < imageWidth - 1; x++) //Удаление максимума и минимума
                for (int y = 0; y < imageHeight - 1; y++)
                {
                    viewImage[y, x] = new Gray(Math.Abs(imageSobel1[x, y]) + Math.Abs(imageSobel2[x, y]));
                }

            imageList1.Images.Add(viewImage.Bitmap);
            listView1.Items.Add("Собель для исходного изображения", 3);


            //Однородная усредняющая фильтрация
            double[,] imageAvrFilter = new double[imageWidth, imageHeight];
            for (int i = 2; i < (imageWidth - 2); i++)
                for (int j = 2; j < (imageHeight - 2); j++)
                    for (int x = -2; x <= 2; x++)
                        for (int y = -2; y <= 2; y++)
                        {
                            Gray color = viewImage[j + y, i + x];
                            imageAvrFilter[i, j] += color.Intensity;
                        }

            for (int x = 0; x < imageWidth - 1; x++) //Удаление максимума и минимума
                for (int y = 0; y < imageHeight - 1; y++)
                {
                    viewImage[y, x] = new Gray(imageAvrFilter[x, y] / 25);
                }
            imageList1.Images.Add(viewImage.Bitmap);
            listView1.Items.Add("Однородная усредняющая фильтрация", 4);

            //Перемножение Лапласиана и сглаженного градиента
            Image<Gray, byte> imageLaplasianAndGrad = imageInputOrLaplasian.And(viewImage);
            imageList1.Images.Add(imageLaplasianAndGrad.Bitmap);
            listView1.Items.Add("Перемножение Лапласиана и сглаженного градиента", 5);

            //Сложение исходного изображения с изображением выше
            Image<Gray, byte> imageInputOrLaplasianAndGrad = imageLaplasianAndGrad.Or(inputImage);
            imageList1.Images.Add(imageInputOrLaplasianAndGrad.Bitmap);
            listView1.Items.Add("Сложение исходного изображения с изображением выше", 6);


            //Результат
            int[] hist = new int[256];

            for (int y = 0; y < imageInputOrLaplasianAndGrad.Height; y++)
                for (int x = 0; x < imageInputOrLaplasianAndGrad.Width; x++)
                {
                    hist[(int)imageInputOrLaplasianAndGrad[y, x].Intensity]++;
                }

            Image<Gray, byte> imageOutput = new Image<Gray, byte>(imageWidth, imageHeight);
            for (int y = 0; y < imageInputOrLaplasianAndGrad.Height; y++)
                for (int x = 0; x < imageInputOrLaplasianAndGrad.Width; x++)
                {
                    imageOutput[y, x] = new Gray(Math.Pow((imageInputOrLaplasianAndGrad[y, x].Intensity) * 255, .5));
                }
            imageList1.Images.Add(imageOutput.Bitmap);
            listView1.Items.Add("Результат", 7);
        }
    }
}
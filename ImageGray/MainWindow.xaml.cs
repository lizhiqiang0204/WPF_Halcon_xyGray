using HalconDotNet;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageGray
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region  成员
        HTuple ImageWidth, ImageHeight;
        private double RowDown;//鼠标按下时的行坐标
        private double ColDown;//鼠标按下时的列坐标
        HObject ho_image;      //图像变量
        HObject ho_image_gray;      //灰度图像变量

        private bool KeyHandDown = false;
        #endregion

        #region 构造方法
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region 缩放图像
        private void HMouseWheel(object sender, HMouseEventArgsWPF e)
        {
            HTuple Zoom, Row, Col, Button;
            HTuple Row0, Column0, Row00, Column00, Ht, Wt, r1, c1, r2, c2;

            if (ho_image_gray == null)
                return;
            if (e.Delta > 0)
            {
                Zoom = 1.5;
            }
            else
            {
                Zoom = 0.5;
            }
            HOperatorSet.GetMposition(hWindowControl1.HalconWindow, out Row, out Col, out Button);
            HOperatorSet.GetPart(hWindowControl1.HalconWindow, out Row0, out Column0, out Row00, out Column00);
            Ht = Row00 - Row0;
            Wt = Column00 - Column0;
            if (Ht * Wt < 32000 * 32000 || Zoom == 1.5)//普通版halcon能处理的图像最大尺寸是32K*32K。如果无限缩小原图像，导致显示的图像超出限制，则会造成程序崩溃
            {
                r1 = (Row0 + ((1 - (1.0 / Zoom)) * (Row - Row0)));
                c1 = (Column0 + ((1 - (1.0 / Zoom)) * (Col - Column0)));
                r2 = r1 + (Ht / Zoom);
                c2 = c1 + (Wt / Zoom);
                HOperatorSet.SetPart(hWindowControl1.HalconWindow, r1, c1, r2, c2);
                HOperatorSet.ClearWindow(hWindowControl1.HalconWindow);
                HOperatorSet.DispObj(ho_image_gray, hWindowControl1.HalconWindow);
            }
        }
        #endregion

        #region 鼠标抬起，实现图像移动
        private void HMouseUp(object sender, HMouseEventArgsWPF e)
        {
            KeyHandDown = false;
            Mouse.OverrideCursor = null;
            HTuple row1, col1, row2, col2, Row, Column, Button;
            HOperatorSet.GetMposition(hWindowControl1.HalconWindow, out Row, out Column, out Button);
            double RowMove = Row - RowDown;   //鼠标弹起时的行坐标减去按下时的行坐标，得到行坐标的移动值
            double ColMove = Column - ColDown;//鼠标弹起时的列坐标减去按下时的列坐标，得到列坐标的移动值
            HOperatorSet.GetPart(hWindowControl1.HalconWindow, out row1, out col1, out row2, out col2);//得到当前的窗口坐标
            HOperatorSet.SetPart(hWindowControl1.HalconWindow, row1 - RowMove, col1 - ColMove, row2 - RowMove, col2 - ColMove);//这里可能有些不好理解。以左上角原点为参考点
            HOperatorSet.ClearWindow(hWindowControl1.HalconWindow);
            if (ho_image_gray != null)
            {
                HOperatorSet.DispObj(ho_image_gray, hWindowControl1.HalconWindow);
            }
            else
            {
                MessageBox.Show("请加载一张图片");
            }
        }
        #endregion

        #region 加载图像
        private void BtnClickLoadPic(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "JPEG文件|*.jpg*|BMP文件|*.bmp*|TIFF文件|*.tiff*";
            openFileDialog.Filter = "所有图像文件 | *.bmp; *.pcx; *.png; *.jpg; *.gif;*.tif; *.ico; *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf";
            if (openFileDialog.ShowDialog() == true)
            {
                HTuple ImagePath = openFileDialog.FileName;
                HOperatorSet.ReadImage(out ho_image, ImagePath);
                HOperatorSet.Rgb1ToGray(ho_image, out ho_image_gray);
                HOperatorSet.DispObj(ho_image_gray, hWindowControl1.HalconWindow);
                HOperatorSet.GetImageSize(ho_image_gray, out ImageWidth, out ImageHeight);
            }
        }
        #endregion

        #region 全屏
        private void BtnClickFull(object sender, RoutedEventArgs e)
        {
            HOperatorSet.SetPart(hWindowControl1.HalconWindow, 0, 0, ImageHeight - 1, ImageWidth - 1);
            HOperatorSet.ClearWindow(hWindowControl1.HalconWindow);
            HOperatorSet.DispObj(ho_image_gray, hWindowControl1.HalconWindow);
        }
        #endregion

        #region 鼠标按下，记录当前坐标值
        private void HMouseDown(object sender, HMouseEventArgsWPF e)
        {
            HTuple Row, Column, Button;
            HOperatorSet.GetMposition(hWindowControl1.HalconWindow, out Row, out Column, out Button);
            RowDown = Row;    //鼠标按下时的行坐标
            ColDown = Column; //鼠标按下时的列坐标
            KeyHandDown = true;
            Mouse.OverrideCursor = Cursors.Hand;
            //this.ForceCursor = false;
            //hWindowControl1.Cursor = Cursors.Hand;
            //Cursor = Cursors.Hand;

        }
        #endregion

        #region 获取坐标灰度值
        private void HMouseMove(object sender, HMouseEventArgsWPF e)
        {
            HTuple Row, Column, Button, pointGray;
            HOperatorSet.GetMposition(hWindowControl1.HalconWindow, out Row, out Column, out Button);              //获取当前鼠标的坐标值
            if (ho_image_gray == null)
                return;

            if (KeyHandDown == true)
            {
                Mouse.OverrideCursor = Cursors.Hand;//移动图片时鼠标光标会有闪动，暂时还没找到好的解决办法
                HTuple row1, col1, row2, col2;
                HOperatorSet.GetMposition(hWindowControl1.HalconWindow, out Row, out Column, out Button);
                double RowMove = Row - RowDown;   //鼠标弹起时的行坐标减去按下时的行坐标，得到行坐标的移动值
                double ColMove = Column - ColDown;//鼠标弹起时的列坐标减去按下时的列坐标，得到列坐标的移动值
                HOperatorSet.GetPart(hWindowControl1.HalconWindow, out row1, out col1, out row2, out col2);//得到当前的窗口坐标
                HOperatorSet.SetPart(hWindowControl1.HalconWindow, row1 - RowMove, col1 - ColMove, row2 - RowMove, col2 - ColMove);//这里可能有些不好理解。以左上角原点为参考点
                //HOperatorSet.ClearWindow(hWindowControl1.HalconWindow);//如果每次移动一个像素就清空背景刷新会导致图片显示闪动，不清空背景，移动图片会有残影，暂时也没找到好的解决办法
                if (ho_image_gray != null)
                {
                    HOperatorSet.DispObj(ho_image_gray, hWindowControl1.HalconWindow);
                }
                else
                {
                    MessageBox.Show("请加载一张图片");
                }
            }
            else
            {
                if (ImageHeight != null && (Row >= 0 && Row < ImageHeight) && (Column >= 0 && Column < ImageWidth))//设置3个条件项，防止程序崩溃。
                {
                    HOperatorSet.GetGrayval(ho_image_gray, Row, Column, out pointGray);                 //获取当前点的灰度值
                }
                else
                {
                    pointGray = "_";
                }
                String str = String.Format("x:{0}  y:{1}  Gray:{2}", Column, Row, pointGray); //格式化字符串
                txbGray.Text = str;
            }
            
        }
        #endregion
    }
}

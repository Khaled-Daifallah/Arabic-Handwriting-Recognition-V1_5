using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ver1._
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Bitmap pic = new Bitmap(@"e:\letters\test11\isht.bmp");
            Determination det = new Determination(new UnsafeBitmap(pic));
            pic = det.determine();
            pic.Save(@"e:\letters\test11\res_test11.bmp");
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Gdal = OSGeo.GDAL.Gdal;
using Ogr = OSGeo.OGR.Ogr;
using OSGeo.OGR;

namespace GeoTxtToShp {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        List<Data> DataList = new List<Data>();

        private void btnImport_Click(object sender, EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c://";
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "txt文件|*.txt|所有文件|*.*";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == DialogResult.OK) {                
                string[] importTxt = openFileDialog.FileNames;
                foreach (string file in importTxt) {
                    DataRead(file,GetFileName(file));
                }
                MessageBox.Show("共计"+DataList.Count.ToString()+"个地块");
            }
        }

        private string GetFileName(string file) {
            string fileName;
            int i = file.Split('\\').Length;
            fileName = file.Split('\\')[i - 1];
            fileName = fileName.Split('.')[0];
            return fileName;   
        }

        private void DataRead(string file,string fileName) {
            StreamReader sr = new StreamReader(file, Encoding.GetEncoding("gb2312"));
            Data data = new Data();
            string txt = sr.ReadLine();
            while (txt != null) {
                
                if (IsNumber(txt.Split(',')[0])) {
                    Point point = new Point();
                    point.X=double.Parse(txt.Split(',')[5]);
                    point.Y = double.Parse(txt.Split(',')[4]);
                    data.PointList.Add(point);
                } else {
                    if (!string.IsNullOrEmpty(data.PCMC)) {
                        DataList.Add(data);
                    }
                    data = new Data();
                    data.Dkh = txt;
                    data.PCMC = fileName;
                }
                txt = sr.ReadLine();
            }
            DataList.Add(data);
        }


        public  bool IsNumber(string strNumber) {
            //看要用哪種規則判斷，自行修改strValue即可

            //strValue = @"^\d+[.]?\d*$";//非負數字
            string strValue = @"^\d+(\.)?\d*$";//數字
            //strValue = @"^\d+$";//非負整數
            //strValue = @"^-?\d+$";//整數
            //strValue = @"^-[0-9]*[1-9][0-9]*$";//負整數
            //strValue = @"^[0-9]*[1-9][0-9]*$";//正整數
            //strValue = @"^((-\d+)|(0+))$";//非正整數

            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(strValue);
            return r.IsMatch(strNumber);
        }

        private void btnGenerate_Click(object sender, EventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "shp文件|*.shp|所有文件|*.*";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                string saveFile = saveFileDialog.FileName;
                WriteVectorFile(saveFile);
            }

        }

        private void WriteVectorFile(string strVectorFile) {
            SharpMap.GdalConfiguration.ConfigureGdal();
            SharpMap.GdalConfiguration.ConfigureOgr();
            // 为了支持中文路径，请添加下面这句代码  
            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            // 为了使属性表字段支持中文，请添加下面这句  
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");
            Ogr.RegisterAll();

            string strDriverName = "ESRI Shapefile";
            Driver oDriver = Ogr.GetDriverByName(strDriverName);
            if (oDriver == null) {
                MessageBox.Show("%s 驱动不可用！!\n", strVectorFile);
                return;
            }

            DataSource oDS = oDriver.CreateDataSource(strVectorFile, null);
            if (oDS == null) {
                MessageBox.Show("创建矢量文件【%s】失败！\n", strVectorFile);
                return;
            }

            Layer oLayer = oDS.CreateLayer("TestPolygon", null, wkbGeometryType.wkbPolygon, null);
            if (oLayer == null) {
                MessageBox.Show("图层创建失败！\n");
                return;
            }
            //FID
            FieldDefn oFieldID = new FieldDefn("FID", FieldType.OFTInteger);
            oLayer.CreateField(oFieldID, 1);

            //批次
            FieldDefn oPCMC = new FieldDefn("PCMC", FieldType.OFTString);
            oPCMC.SetWidth(100);
            oLayer.CreateField(oPCMC, 1);
            //地块号
            FieldDefn oDKH = new FieldDefn("GKH", FieldType.OFTString);
            oDKH.SetWidth(100);
            oLayer.CreateField(oDKH, 1);

            FeatureDefn oDefn = oLayer.GetLayerDefn();
            int index = 0;
            foreach (Data data in DataList) {
                index++;
                Feature oFeature = new Feature(oDefn);
                oFeature.SetField(0, index);
                oFeature.SetField(1, data.PCMC);
                oFeature.SetField(2, data.Dkh);
                Geometry geomTriangle = Geometry.CreateFromWkt(GetGeometry(data));
                oFeature.SetGeometry(geomTriangle);
                oLayer.CreateFeature(oFeature);
            }
            oDS.Dispose();
            MessageBox.Show("生成完毕");
        }

        private string GetGeometry(Data data) {
            StringBuilder geometry=new StringBuilder();
            geometry.Append("POLYGON ((");
            foreach (Point point in data.PointList) {
                geometry.Append(point.X);
                geometry.Append(" ");
                geometry.Append(point.Y);
                geometry.Append(",");
            }
            //如果最后一个地坐标与第一个点坐标不相同
            if (data.PointList[0].X != data.PointList[data.PointList.Count - 1].X) {
                geometry.Append(data.PointList[0].X);
                geometry.Append(" ");
                geometry.Append(data.PointList[0].Y);
                geometry.Append(",");
            }
            geometry.Remove(geometry.Length - 1,1);
            geometry.Append("))");
            return geometry.ToString();
        }



    }
}

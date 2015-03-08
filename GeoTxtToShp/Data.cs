using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GeoTxtToShp {
    public class Data {
        public Data() {
            _pointList=new List<Point>();
        }



        List<Point> _pointList;

        public List<Point> PointList {
            get {
                return _pointList;
            }
            set {
                _pointList = value;
            }
        }



        string _pcmc;

        public string PCMC {
            get {
                return _pcmc;
            }
            set {
                _pcmc = value;
            }
        }

        string _dkh;

        public string Dkh {
            get {
                return _dkh;
            }
            set {
                _dkh = value;
            }
        }

    }

    public class Point {
        double _x;

        public double X {
            get {
                return _x;
            }
            set {
                _x = value;
            }
        }
        double _y;

        public double Y {
            get {
                return _y;
            }
            set {
                _y = value;
            }
        }
    }
}

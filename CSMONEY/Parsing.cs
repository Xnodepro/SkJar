using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSMONEY
{
    public partial class Parsing : Form
    {
        public Parsing()
        {
            InitializeComponent();
        }
        public struct Dat
        {
            public string Name { get; set; }
            public double PriceMoney { get; set; }
            public double PriceJar { get; set; }
            public double Percent{ get; set; }
        }
        List<Dat> It = new List<Dat>();
        private void button1_Click(object sender, EventArgs e)
        {
            Form.CheckForIllegalCrossThreadCalls = false;

            new System.Threading.Thread(delegate () {
                try
                {
                    string ss = "";
                    foreach (var item in Program.DataJar.items)
                    {
                        var moneyItems = Program.DataMoney.Find(n => (n.Name.Replace(" ", "") == item.name.Replace(" ", "")));
                        if (moneyItems != null)
                        {
                            double tmpMoneyPrice = moneyItems.Price * 0.97;
                            double _Percent = ((tmpMoneyPrice / item.price) - 1)*100;
                            ss = $"Название:{ item.name }  Цена мани:{ tmpMoneyPrice}  Цена Джар:{ item.price}---Процент:{_Percent}" + Environment.NewLine + ss;
                            Dat D = new Dat()
                            {
                                Name = item.name,
                                PriceMoney = tmpMoneyPrice,
                                PriceJar = item.price,
                                Percent = _Percent
                            };
                            It.Add(D);
                        }

                    }
                    var aa = It;
                    textBox1.Text = ss;
                }
                catch (Exception ex) { }
            }).Start();
           
        }
    }
}

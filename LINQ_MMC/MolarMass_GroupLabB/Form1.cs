using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;



namespace MolarMass_GroupLabB
{
    public partial class Form1 : Form
    {
        List<Chemical> baseChemicalList = new List<Chemical>();
        BindingSource BS = new BindingSource();
        public Form1()
        {
            InitializeComponent();
            this.Text = "LINQ ICA";
            dataGridView1.DataSource = BS;
            LoadFile();
        }
        //-------------------------------------------------------------Form Events-------------------------------------------------------------
        private void chemFormBox_TextChanged(object sender, EventArgs e)
        {
            int checkBox = 0;
            var testBig = new Regex(@"([A-Z]{1}[a-z]{0,1})([0-9]{0,2})"); // break on these "|" solid line seperates the statements 
                                                                          // () is for grouping
                                                                          // carrot and dollar means it must start with this case(carrot) and end with that case(dollar sign) respectively
            MatchCollection matches = testBig.Matches(chemFormBox.Text);
            Console.WriteLine(matches);
            foreach (Match m in matches)
            { checkBox += m.Length; }

            JoinDisplayChemicals(matches);

            if (chemFormBox.Text.Length != checkBox)
            {
                totalMassBox.ForeColor = Color.Red;
                totalMassBox.Text = "Wrong";
            }

        }


        private void sortButton_Click(object sender, EventArgs e)
        {
            //When the sort by name button is clicked, data grid is sorted according to element name alphabetically
            SortByName();

        }
        private void sortNumButton_Click(object sender, EventArgs e)
        {
            SortByAtomic();
        }
        private void singlecharButton_Click(object sender, EventArgs e)
        {
            SortBySingle();
        }
        //----------------------------------------------------Methods-------------------------------------------------
        /**************************************************************************
         * Load File: Loads CSV file and adds elements to a collection for
         *            manipulation.
         * ************************************************************************/
        private void LoadFile()
        {
            List<string> listOfLines = new List<string>();
            List<string> listOfValues = new List<string>();
            int counter = 0;
            try
            {
                StreamReader reader = new StreamReader(@"PeriodicTable.csv");
                using (reader = new StreamReader(@"PeriodicTable.csv"))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split(',');
                        if (counter > 0)            // special case so title line of csv file does not cause error
                        {
                            Chemical adder = new Chemical(Convert.ToInt32(values[0]), values[2].Trim(' '), values[1].Trim(' '), Convert.ToDouble(values[3]));
                            baseChemicalList.Add(adder);
                        }

                        counter++;
                    }

                    BS.DataSource = baseChemicalList;

                }

            }
            catch (Exception e)
            {
                MessageBox.Show("Error in Load File: " + e.Message, "MMC_B", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /**************************************************************************
         * SortByName: Clears current data grid, sorts list of elements by name
         *             and outputs to the data grid
         * ************************************************************************/
        private void SortByName()
        {
            chemFormBox.Clear(); totalMassBox.Clear();
            List<Chemical> copyChemicalList = baseChemicalList;
            BS.DataSource = from element in copyChemicalList orderby element.chemicalName select element;
        }
        /**************************************************************************
         * SortByAtomic: Clears current data grid, sorts list of elements by their
         *               respective atomic number and outputs it to the grid
         * ************************************************************************/
        private void SortByAtomic()
        {
            chemFormBox.Clear(); totalMassBox.Clear();
            List<Chemical> copyChemicalList = baseChemicalList;
            BS.DataSource = from element in copyChemicalList orderby element.atomicNum select element;
        }

        /**************************************************************************
         * SortBySingle: Clears current data grid, Sorts the list of elements such
         *               that they only include the single character elements
         * ************************************************************************/
        private void SortBySingle()
        {
            chemFormBox.Clear(); totalMassBox.Clear();
            List<Chemical> copyChemicalList = baseChemicalList;
            BS.DataSource = from element in copyChemicalList orderby element.chemicalSymbol.Length, element.chemicalSymbol select element;
        }
        /**************************************************************************
         * CalculateToMass: Calculates the total molar mass of a chemical compound
         *                  and outputs the value toa text field.
         **************************************************************************/

        private void JoinDisplayChemicals(MatchCollection matches)
        {
            
            Dictionary<string, int> matchDictionary = new Dictionary<string, int>();

            List<string> symbolList = new List<string>();       // to be list of the all the chemical symbols
            baseChemicalList.ForEach(x => symbolList.Add(x.chemicalSymbol));    // adding symbols
            double totalSum = 0;          // total mass for calculation and display reset every event
            //Dictionary<Chemical, int> resultDictionary = new Dictionary<Chemical, int>();
            bool incorrectValue = false;

            foreach (Match m in matches)                // iterate through match collection
            {
                if (m.Groups[m.Groups.Count - 1].Value.ToString() != "")  // check if number group is empty
                {
                    if (matchDictionary.ContainsKey(m.Groups[1].Value.ToString()))      // check for chemicals existence in dictionary
                        matchDictionary[m.Groups[1].Value.ToString()] += int.Parse(m.Groups[2].Value.ToString());  // add multipler to value

                    else
                        matchDictionary.Add(m.Groups[1].Value.ToString(), int.Parse(m.Groups[2].Value.ToString())); // create chemcial key and multipler value
                }
                else      // no multipling number
                {
                    if (matchDictionary.ContainsKey(m.Groups[1].Value.ToString()))  // check for existence
                        matchDictionary[m.Groups[1].Value.ToString()]++;        // increment multipler value

                    else
                        matchDictionary.Add(m.Groups[1].Value.ToString(), 1);       // create chemical and multipler number
                }

            }

            var rChems = from c in baseChemicalList                     // search base chemicals
                         join m in matchDictionary on c.chemicalSymbol equals m.Key // join on match dictionary
                         select new
                         {
                             Element = c.chemicalName,      // the stuff
                             Count = m.Value,
                             Mass = c.chemicalMass,
                             TotalMass = c.chemicalMass * m.Value


                         };
            
            rChems.ToList().ForEach(x => totalSum += x.TotalMass);  // increase total sum for display accoridingly

            totalMassBox.ForeColor = Color.Green; totalMassBox.Text = totalSum.ToString() + " g/mol"; // display totalsum of total masses

            BS.DataSource = rChems;         // datasource for grid view

            foreach (KeyValuePair<string, int> mD in matchDictionary)   
            {
                if (!symbolList.Contains(mD.Key))   // check for incorrect match for chemicals
                    incorrectValue = true;
            }
            if (incorrectValue) // if and incorrect value is found set error messege
            {
                totalMassBox.ForeColor = Color.Red;
                totalMassBox.Text = "Wrong";
            }
        } 
      
    }



}

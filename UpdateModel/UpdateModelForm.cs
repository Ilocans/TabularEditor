using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabularEditor.TOMWrapper;
using System.Windows.Forms;

namespace UpdateModel
{


    public partial class UpdateModelForm : Form
    {
        private Model Model;
        public UpdateModelForm(Model myModel)
        {
            InitializeComponent();
            Model = myModel;
        }


        private bool CheckHeader(string header)
        {
            var tsvColumns = header.Split('\t');
            if (tsvColumns[1] == "Name"
                && tsvColumns[2] == "ObjectType"
                && tsvColumns[3] == "Parent"
                && tsvColumns[4] == "Description"
                && tsvColumns[5] == "FormatString"
                && tsvColumns[6] == "DisplayFolder"
                && tsvColumns[7] == "SortByColumn"
                && tsvColumns[8] == "DataType"
                && tsvColumns[9] == "Expression"
                && tsvColumns[10] == "IsHidden"
                && tsvColumns[11] == "SummarizeBy"
                && tsvColumns[12] == "IsKey"
                && tsvColumns[13] == "IsNullable"
                && tsvColumns[14] == "IsUnique"
                )
             return true; 
            else
                return false;
        }

        private void UpdateModel(string fileName)
        {
            try
            {

                //fileName = "C:\\Temp\\contoso.tsv";
                // Construct a list of all visible columns and measures:
                var objects = Model.AllMeasures.Cast<ITabularNamedObject>().Concat(Model.AllColumns);
                // Get their properties in TSV format (tabulator-separated):
                //var tsv = ExportProperties(objects, "Name,ObjectType,Parent,Description,FormatString,DisplayFolder,SortByColumn,DataType,Expression,IsHidden,SummarizeBy,IsKey,IsNullable ,IsUnique");
                var tsv = ReadFile(fileName);

                // Split le fichier en ligne par CR & LF 
                var tsvRows = tsv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (CheckHeader(tsvRows[0]) == false)
                    throw new InvalidFileFormatException();
                // Pour chaque element du dictionnaire
                foreach (var row in tsvRows.Skip(1))
                {
                    var tsvColumns = row.Split('\t');
                    var name = tsvColumns[1];
                    var objectType = tsvColumns[2];
                    var expression = tsvColumns[9];
                    var tablename = tsvColumns[3].Split('.')[1];
                    //.Split('.')[1];
                    var targetTable = Model.Tables[tablename];
                    Console.WriteLine(tablename);

                    // Si c est une mesure
                    if (objectType.Equals("Measure"))
                    {
                        //Console.WriteLine(name);
                        var newMeasure = targetTable.AddMeasure(
                                        name,           // Name
                                        expression,     // DAX expression
                                        tsvColumns[6]   // Display Folder
                                        );
                        newMeasure.Description = tsvColumns[4];
                        newMeasure.IsHidden = Convert.ToBoolean(tsvColumns[10]);
                        newMeasure.FormatString = tsvColumns[5];
                    }



                    // Si colonne
                    else if (objectType.Equals("Column"))
                    {
                        TabularEditor.TOMWrapper.Column column;
                        //si colonne calculée
                        if (!expression.Equals(""))
                        {
                            column = targetTable.AddCalculatedColumn(
                                     name,// Name
                                     expression,    // DAX expression
                                     tsvColumns[6]                       // Display Folder
                                     );
                        }
                        else
                        {
                            column = targetTable.Columns[name];
                        }


                        column.Description = tsvColumns[4];
                        column.FormatString = tsvColumns[5];
                        column.DisplayFolder = tsvColumns[6];
                        column.IsHidden = Convert.ToBoolean(tsvColumns[10]);
                        //AggregateFunction t = AggregateFunction.Default;

                        // Gestion aggregate function
                        switch (tsvColumns[11])
                        {
                            case "None":
                                column.SummarizeBy = AggregateFunction.None;
                                break;
                            case "Default":
                                column.SummarizeBy = AggregateFunction.Default;
                                break;
                            case "Min":
                                column.SummarizeBy = AggregateFunction.Min;
                                break;
                            case "Max":
                                column.SummarizeBy = AggregateFunction.Max;
                                break;
                            case "Sum":
                                column.SummarizeBy = AggregateFunction.Sum;
                                break;
                            case "Count":
                                column.SummarizeBy = AggregateFunction.Count;
                                break;
                            case "Average":
                                column.SummarizeBy = AggregateFunction.Average;
                                break;
                            case "DistinctCount":
                                column.SummarizeBy = AggregateFunction.DistinctCount;
                                break;
                            default:
                                column.SummarizeBy = AggregateFunction.Default;
                                break;
                        }

                        //t = new AggregateFunction().Default; 
                        //t = new AggregateFunction().Default;

                        //column.SummarizeBy = SummerizedBy();


                        column.IsKey = Convert.ToBoolean(tsvColumns[12]);
                        column.IsNullable = Convert.ToBoolean(tsvColumns[13]);
                        column.IsUnique = Convert.ToBoolean(tsvColumns[14]);
                    }
                }

            }
            catch (IOException)
            {
            }
            catch (InvalidFileFormatException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void button1_Click(object sender, EventArgs e)
        {
            UpdateModel(textBox1.Text.ToString());
            this.Hide();
        }


        private string ReadFile(string filePath)
        {
            using (var fileStream = new System.IO.FileStream(filePath,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite))
            using (var textReader = new System.IO.StreamReader(fileStream))
            {
                return textReader.ReadToEnd();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int size = -1;
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
                textBox1.Text = openFileDialog1.FileName;

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}

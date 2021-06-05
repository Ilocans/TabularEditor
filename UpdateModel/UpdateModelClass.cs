using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TabularEditor.TOMWrapper;
using System.Windows.Forms;


namespace UpdateModel
{
    public class UpdateModelClass : ITabularEditorPlugin
    {
        TabularModelHandler Handler;
        Model Model;

        public void Init(TabularModelHandler handler)
        {

            Handler = handler;
            Model = handler.Model;
            //form = new VisualizeRelationshipsForm();
            //form.Model = model;



        }

        public void RegisterActions(Action<string, Action> registerCallback)
        {
            registerCallback("Update Model", UpdateModel);
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

        private void UpdateModel()
        {
            if (Handler != null)
            {
                var fileName = "C:\\Temp\\contoso.tsv";
                // Construct a list of all visible columns and measures:
                var objects = Model.AllMeasures.Cast<ITabularNamedObject>().Concat(Model.AllColumns);
                // Get their properties in TSV format (tabulator-separated):
                //var tsv = ExportProperties(objects, "Name,ObjectType,Parent,Description,FormatString,DisplayFolder,SortByColumn,DataType,Expression,IsHidden,SummarizeBy,IsKey,IsNullable ,IsUnique");
                var tsv = ReadFile(fileName);

                // Split le fichier en ligne par CR & LF 
                var tsvRows = tsv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
                        Console.WriteLine(name);
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
        }
    }

}

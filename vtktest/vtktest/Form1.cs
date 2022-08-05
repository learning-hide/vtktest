using Kitware.VTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace vtktest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<vtkActor> actors = new List<vtkActor>();

        private void Form1_Load(object sender, EventArgs e)
        {
            vtu("block.vtu");
            var colms = getColumns(0);
            foreach (var colm in colms)
            {
                column.Items.Add(colm);
            }
        }


        public  void vtu(string filePath)
        {
            renderWindowControl1.Invalidate();
            vtkXMLUnstructuredGridReader reader = vtkXMLUnstructuredGridReader.New();
            reader.SetFileName(filePath);
            reader.Update();
            vtkRenderWindow renderWindow = renderWindowControl1.RenderWindow;
            vtkRenderer renderer = renderWindow.GetRenderers().GetFirstRenderer();
            vtkDataSetMapper mapper = vtkDataSetMapper.New();
            mapper.SetInputConnection(reader.GetOutputPort());
            actors.Add(new vtkActor());
            vtkActor actor = actors.Last();
            actor.SetMapper(mapper);
            renderer.AddActor(actor);
            renderer.ResetCamera();
            renderWindowControl1.Validate();

        }
 
        private void column_SelectedIndexChanged(object sender, EventArgs e)
        {
            renderWindowControl1.Invalidate();
            for (int i = 0; i < actors.Count; i++)
            {
                var colm = column.GetItemText(column.SelectedItem);
                setColors(i, colm);
            }
            renderWindowControl1.Validate();
        }

        public void setColors(int actorIndex, string column)
        {
            List<string> colums = new List<string>();
            vtkActor actor = actors[actorIndex];
            var mapper = actor.GetMapper();
            var output = mapper.GetInputAsDataSet();


            var colors = vtkUnsignedCharArray.New();
            colors.SetNumberOfComponents(3);
            colors.SetName("Colors");

            var cell = output.GetCellData();
            var num1 = cell.GetNumberOfComponents();
            var num2 = cell.GetNumberOfArrays();


            for (int i = 0; i < num1; i++)
            {
                var cells = cell.GetArray(i);
                if (cells == null) continue;
                if (column != cell.GetArrayName(i)) continue;
                var range = cell.GetArray(i).GetRange();


                //Console.WriteLine("\n\n\n");
                for (int j = 0; j < cells.GetNumberOfTuples(); j++)
                {
                    var val = cells.GetComponent(i, j);
                    var per = val / cell.GetArray(i).GetRange()[1];
                    if (!isNotNan(val)) per = 0;
                    Console.WriteLine($"{val}:{per}");
                    // Console.WriteLine(val);
                    var c =  InterpolateColor(per); //get color
                    var R = c.R;
                    var G = c.G;
                    var B = c.B;
                    colors.InsertNextTuple3((double)R, (double)G, (double)B);
                    colors.InsertNextTuple3((double)R, (double)G, (double)B);
                }

            }
            output.GetPointData().SetScalars(colors);
            output.GetCellData().SetScalars(colors);
        }

        public bool isNotNan(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        Color[] colors = new Color[] { Color.Red, Color.Lime, Color.Orange, Color.Blue };

        public Color InterpolateColor(double x)
        {
            double r = 0.0, g = 0.0, b = 0.0;
            double total = 0.0;
            double step = 1.0 / (double)(colors.Length - 1);
            double mu = 0.0;
            double sigma_2 = 0.035;
            foreach (Color color in colors)
            {
                total += Math.Exp(-(x - mu) * (x - mu) / (2.0 * sigma_2)) / Math.Sqrt(2.0 * Math.PI * sigma_2);
                mu += step;
            }

            mu = 0.0;
            foreach (Color color in colors)
            {
                double percent = Math.Exp(-(x - mu) * (x - mu) / (2.0 * sigma_2)) / Math.Sqrt(2.0 * Math.PI * sigma_2);
                mu += step;

                r += color.R * percent / total;
                g += color.G * percent / total;
                b += color.B * percent / total;
            }

            return Color.FromArgb(255, (int)r, (int)g, (int)b);
        }



        public double[] getRange(int actorIndex, string column)
        {
            List<string> colums = new List<string>();
            vtkActor actor = actors[actorIndex];
            var output = actor.GetMapper().GetInputAsDataSet();
            var cell = output.GetCellData();
            for (int i = 0; i < cell.GetNumberOfArrays(); i++)
            {
                var colm = cell.GetArrayName(i);
                var arr = cell.GetArray(i);
                if (column == colm)
                {
                    if (arr == null) return null;
                    return arr.GetRange();
                }
            }
            return null;
        }

        public List<string> getColumns(int actorIndex)
        {
            List<string> colums = new List<string>();
            vtkActor actor = actors[actorIndex];
            var output = actor.GetMapper().GetInputAsDataSet();
            var cell = output.GetCellData();
            for (int i = 0; i < cell.GetNumberOfArrays(); i++)
            {
                var colm = cell.GetArrayName(i);
                if (colm != null) colums.Add(colm);
            }
            return colums;
        }


    } 
}

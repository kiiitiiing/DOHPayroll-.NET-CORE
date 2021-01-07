using DinkToPdf;
using DinkToPdf.Contracts;
using DOHPayroll.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOHPayroll
{
    public class PDFEz
    {
        private IConverter _converter;
        public List<Table> Tables { get; set; }
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public  Unit Unit{ get; set; }
        public ColorMode ColorMode { get; set; }
        public Orientation Orientation { get; set; }
        public PechkinPaperSize PaperSize { get; set; }
        public string Title { get; set; }
        public bool PagesCount { get; set; }
        private string Content { get; set; }
        public PDFEz(IConverter converter)
        {
            _converter = converter;
            Unit = Unit.Inches;
            Top = 1;
            Bottom = 1;
            Left = 1;
            Right = 1;
            ColorMode = ColorMode.Color;
            Orientation = Orientation.Portrait;
            PagesCount = true;
            Tables = new List<Table>();
        }

        public void AddTable(Table table)
        {
            Tables.Add(table);
        }

        public void AddTables(params Table[] tables)
        {
            Tables.AddRange(tables.ToList());
        }

        public byte[] GeneratePdf()
        {
            SetContent();
            new CustomAssemblyLoadContext().LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            var globalSettings = new GlobalSettings
            {
                ColorMode = this.ColorMode,
                Orientation = this.Orientation,
                PaperSize = this.PaperSize,
                Margins = new MarginSettings { Top = Top, Bottom = Bottom, Left = Left, Right = Right, Unit = this.Unit },
                DocumentTitle = Title
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = PagesCount,
                HtmlContent = Content,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "pdf_payslip.css") }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            return _converter.Convert(pdf);
        }

        public void SetContent()
        {
            foreach(var content in Tables)
            {
                Content += content.GetHtmlContent();
            }
        }
    }

    public class Table
    {
        private int NoCol { get; set; }
        public string Content { get; set; }
        public int RowCtr { get; set; }
        public List<Row> Rows { get; private set; }
        public Table(int ColNumber)
        {
            NoCol = ColNumber;
            Rows = new List<Row>();
            RowCtr = 0;
        }
        public Table(int ColNumber, params Row[] rows)
        {
            NoCol = ColNumber;
            Rows = rows.ToList();
            RowCtr = rows.Length;
        }

        public void AddRow(Row row)
        {
            Rows.Add(row);
            RowCtr++;
        }

        public void AddRows(IEnumerable<Row> rows)
        {
            Rows.AddRange(rows);
            RowCtr += rows.Count();
        }



        public string GetHtmlContent()
        {
            var html = new StringBuilder();
            //table start
            html.Append(@"
            <table class='table'>
                <tbody>");
            html.AppendFormat(@"
            <tr>
                <td colspan='{0}'></td>
            </tr>", NoCol);
            foreach (var row in Rows)
            {
                //row start
                html.Append("<tr>");
                foreach (var column in row.Columns)
                {
                    html.Append(column.GenerateColumn());
                }
                //row end
                html.Append("</tr>");
            }
            //table end
            html.Append(@"
                </tbody>
            </table>");

            return html.ToString();
        }
    }

    public class Row
    {
        public List<Column> Columns { get;  private set; }
        public Row()
        {
            Columns = new List<Column>();
        }

        public Row(params Column[] columns)
        {
            Columns = columns.ToList();
        }

        public void AddColumn(Column column)
        {
            Columns.Add(column);
        }
    }


    public class Column
    {
        #region CLASSES
        public int FontSize { get; set; }
        public string FontName { get; set; }
        public Color FontColor { get; set; }
        public bool Bold { get; set; }
        public bool Underline { get; set; }
        public bool Italic { get; set; }
        public int ColSpan { get; set; }
        public int RowSpan { get; set; }
        public List<int> Borders { get; set; }
        public int BorderThickness { get; set; }
        #endregion
        public int Padding { get; set; }
        public int PaddingLeft { get; set; }
        public int PaddingRight { get; set; }
        public int PaddingTop { get; set; }
        public int PaddingBottom { get; set; }
        public string Content { get; set; }
        public Color BorderColor { get; set; }
        public bool Rotate { get; set; }
        public string TextAlignment { get; set; }
        public string TextVerticalAlignment { get; set; }
        private List<string> Classes { get; set; }
        public Column(string Content)
        {
            this.Content = Content;
            FontSize = 11;
            FontName = "Arial";
            FontColor = Color.Black;
            Bold = false;
            Underline = false;
            Italic = false;
            Borders = new List<int>();
            BorderColor = Color.Black;
            ColSpan = 1;
            RowSpan = 1;
            Rotate = false;
            Classes = new List<string>();
            TextAlignment = Alignment.LEFT;
            TextVerticalAlignment = VerticalAlignment.MIDDLE;
            Padding = 7;
            PaddingLeft = 0;
            PaddingRight = 0;
            PaddingTop = 0;
            PaddingBottom = 0;
            BorderThickness = 1;
        }

        public string GenerateColumn()
        {
            var html = new StringBuilder();
            //td start
            html.AppendFormat("<td {0} {1} {2}>", GetClasses(), ColRowSpan(), GetStyles());

            html.Append(Content);
            //td end
            html.Append("</td>");

            return html.ToString();
        }

        public string ColRowSpan()
        {
            var span = "colspan='" + ColSpan + "' rowspan='" + RowSpan + "'";
            return span;
        }
        public string GetStyles()
        {
            var styles = new StringBuilder();
            styles.Append("style='");
            styles.AppendFormat(@"font-size: {0}px !important; ", FontSize);
            styles.AppendFormat(@"font-family: {0}; ", FontName);
            styles.AppendFormat(@"color: {0} !important; ", ToHex(FontColor));
            styles.AppendFormat(@"font-weight: {0} !important; ", Bold ? "bold" : "normal");
            styles.AppendFormat(@"text-decoration: {0} !important; ", Underline ? "underline" : "none");
            styles.AppendFormat(@"font-style: {0} !important; ", Italic ? "italic" : "normal");
            styles.AppendFormat(@"text-align: {0}; ", TextAlignment);
            styles.AppendFormat(@"vertical-align: {0}; ", TextVerticalAlignment);
            if (PaddingLeft == 0 && PaddingRight == 0 && PaddingTop == 0 && PaddingBottom == 0)
                styles.AppendFormat(@"padding: {0};", Padding);
            else
            {
                styles.AppendFormat(@"padding-left: {0}", Padding + PaddingLeft);
                styles.AppendFormat(@"padding-right: {0}", Padding + PaddingRight);
                styles.AppendFormat(@"padding-top: {0}", Padding + PaddingTop);
                styles.AppendFormat(@"padding-bottom: {0}", Padding + PaddingBottom);
            }
            foreach(var border in Borders)
            {
                if (border == 1)
                    styles.AppendFormat("border-style: solid; border-width: {0}px; ", BorderThickness);
                else
                {
                    if (border == 4)
                        styles.AppendFormat("border-left-style: solid; border-left-width: {0}px; ", BorderThickness);
                    if (border == 6)
                        styles.AppendFormat("border-right-style: solid; border-right-width: {0}1px; ", BorderThickness);
                    if (border == 8)
                        styles.AppendFormat("border-top-style: solid; border-top-width: {0}px; ", BorderThickness);
                    if (border == 2)
                        styles.AppendFormat("border-bottom-style: solid; border-bottom-width: {0}px; ", BorderThickness);
                }
            }

            styles.Append("'");

            return styles.ToString();
        }

        public string GetClasses()
        {
            if (Rotate)
                Classes.Add(Styles.Rotate);
            var listedClass = "";
            foreach (var clss in Classes)
            {
                listedClass += clss +" ";
            }

            return "class='" + listedClass + "'";
        }

        public string ToHex(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public static Column Blank()
        {
            var blank = new Column("i") 
            {
                FontColor = Color.White
            };
            return blank;
        }

        public static Column Blank(int colSpan)
        {
            var blank = new Column("i")
            {
                FontColor = Color.White,
                ColSpan = colSpan
            };
            return blank;
        }
    }

    public class Styles
    {
        public const string Rotate = "rotate";
    }

    public class Borders
    {
        public const int All = 1;
        public const int Left = 4;
        public const int Right = 6;
        public const int Top = 8;
        public const int Bottom = 2;
    }

    public class Alignment
    {
        public const string CENTER = "center";
        public const string END = "end";
        public const string JUSTIFY = "justify";
        public const string LEFT = "left";
        public const string RIGHT = "right";
        public const string START = "start";
        public const string INHERIT = "inherit";
        public const string INITIAL = "initial";
        public const string UNSET = "undset";
    }

    public class VerticalAlignment
    {

        public const string BOTTOM = "bottom";
        public const string MIDDLE = "middle";
        public const string SUB = "sub";
        public const string SUPER = "super";
        public const string TOP = "top";
    }
}

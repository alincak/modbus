using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Excel;

namespace modbus
{
    #region Global Code Blogs...
    /// <summary>
    /// Dosya tipleri.
    /// </summary>
    public enum IFileType
    {
        Pdf = 0,
        Excel = 1
    }
    #endregion

    /// <summary>
    /// Uygulama içi çıktı alınır.
    /// </summary>
    public class Export
    {
        private System.Drawing.Image m_HeadImage;

        /// <summary>
        /// PDF başlığında görülecek resim set edilir. 
        /// </summary>
        public System.Drawing.Image SetHeadImage
        {
            set { m_HeadImage = value; }
        }

        private string m_path = null;
        private DataGridView m_dataGridView = null;
        private IFileType m_FileType;

        private string[] reportInfo;

        private Microsoft.Office.Interop.Excel.Application excelSheet;


        /// <summary>
        /// Constructor iki parametre alır.
        /// </summary>
        /// <param name="dataGridView">Çıktısı alınacak grid nesnesi.</param>
        /// <param name="_FileType">Çıktısı alınacak dosya tipi.</param>
        public Export(string[] report, DataGridView dataGridView, IFileType _FileType)
        {
            m_dataGridView = dataGridView;
            m_FileType = _FileType;
            reportInfo = report;
        }

        /// <summary>
        /// Constructor iki parametre alır.
        /// </summary>
        /// <param name="dataGridView">Çıktısı alınacak grid nesnesi.</param>
        /// <param name="_FileType">Çıktısı alınacak dosya tipi.</param>
        /// <param name="path">Çıktısı alınacak dosyanın kaydedilecek lokasyon.</param>
        public Export(string path, string[] report, DataGridView dataGridView, IFileType _FileType)
        {
            m_path = path;
            m_dataGridView = dataGridView;
            m_FileType = _FileType;
            reportInfo = report;
        }

        /// <summary>
        /// Constructor yolu ile işlemler yapılır. İşlemi tamamlamak için kullanılır.
        /// </summary>
        /// <returns>Başarılı ise TRUE, başarısız ise FALSE</returns>
        public bool ToExport()
        {
            switch (m_FileType)
            {
                case IFileType.Pdf:
                    return toPdf();
                case IFileType.Excel:
                    return toExcel();
                default:
                    return false;
            }
        }

        private void PdfHead(PdfPTable pdfTable, iTextSharp.text.Font iFont)
        {
            pdfTable.DefaultCell.Padding = 2;
            pdfTable.WidthPercentage = 100;
            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
            pdfTable.SpacingBefore = 50;
            pdfTable.SpacingAfter = 3;

            System.Drawing.Image image = m_HeadImage;
            iTextSharp.text.Image _pdfImage =
                                        iTextSharp.text.Image.GetInstance(image, System.Drawing.Imaging.ImageFormat.Jpeg);

            PdfPCell cell1 = new PdfPCell(iTextSharp.text.Image.GetInstance(_pdfImage), true)
            { Border = PdfPCell.BOTTOM_BORDER };
            pdfTable.AddCell(cell1);


            string text = "Report No : " + reportInfo[0] + "\n \n" +
                          "Tester Name : " + reportInfo[1] + "\n \n" +
                          "Start Time : " + reportInfo[2] + "\n \n" +
                          "End Time : " + reportInfo[3];

            PdfPCell cell = new PdfPCell(new Phrase(text, iFont));
            cell.Border = 0;
            pdfTable.AddCell(cell);


            text = "Note : " + reportInfo[4];
            PdfPCell cellNote = new PdfPCell(new Phrase(text, iFont));
            cellNote.Border = 0;
            pdfTable.AddCell(cellNote);


            PdfPCell time = new PdfPCell(new Phrase(DateTime.Now.ToLongDateString(), iFont));
            time.Border = 0;
            time.HorizontalAlignment = Element.ALIGN_RIGHT;
            pdfTable.AddCell(time);
        }

        private bool toPdf()
        {
            try
            {
                iTextSharp.text.pdf.BaseFont STF_Helvetica_Turkish = iTextSharp.text.pdf.BaseFont.CreateFont("Helvetica", "CP1254", iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);

                iTextSharp.text.Font fontNormal = new iTextSharp.text.Font(STF_Helvetica_Turkish, 12, iTextSharp.text.Font.NORMAL);

                PdfPTable pdfTableHead = new PdfPTable(4);

                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                List<DataGridViewColumn> listVisible = new List<DataGridViewColumn>();
                foreach (DataGridViewColumn col in m_dataGridView.Columns)
                {
                    if (col.Visible)
                        listVisible.Add(col);
                }

                PdfPTable pdfTable = new PdfPTable(listVisible.Count);
                pdfTable.DefaultCell.Padding = 2;
                pdfTable.WidthPercentage = 100;
                pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                pdfTable.DefaultCell.BorderWidth = 1;

                PdfHead(pdfTableHead, fontNormal);

                //Adding Header row
                for (int i = 0; i < listVisible.Count; i++)
                {


                    PdfPCell cell = new PdfPCell(new Phrase(listVisible[i].HeaderText, fontNormal));

                    cell.BackgroundColor = iTextSharp.text.Color.CYAN;
                    pdfTable.AddCell(cell);

                }

                //Adding DataRow
                for (int i = 0; i < m_dataGridView.Rows.Count; i++)
                {
                    for (int j = 0; j < listVisible.Count; j++)
                    {
                        try
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(m_dataGridView.Rows[i].Cells[listVisible[j].Name].Value.ToString(), fontNormal));
                            pdfTable.AddCell(cell);
                        }
                        catch { }
                    }
                }


                using (FileStream stream = new FileStream(m_path, FileMode.Create))
                {
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    pdfDoc.Add(pdfTableHead);
                    pdfDoc.Add(pdfTable);
                    pdfDoc.NewPage();

                    pdfDoc.Close();
                    stream.Close();
                    MessageBox.Show("PDF Created Successfully");
                    return true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF Created UnSuccessfully : " + ex.Message);
                return false;
            }
        }

        private void ExcelHead()
        {

            excelSheet.get_Range("A1", "E1").Merge(); //Satırları birleştirir.
            excelSheet.get_Range("A1", "E1").HorizontalAlignment = XlHAlign.xlHAlignCenter;
            excelSheet.get_Range("A1", "E1").Font.Size = 22;
            excelSheet.get_Range("A1", "E1").Font.Name = "Arial";
            excelSheet.get_Range("A1", "E1").Borders.LineStyle = XlLineStyle.xlContinuous;


            excelSheet.get_Range("A1").Interior.Color = System.Drawing.Color.SkyBlue;
            excelSheet.Cells[1, 1] = "REPORT";


            //No
            excelSheet.Cells[2, 1] = "Report No";
            excelSheet.get_Range("A2").Interior.Color = System.Drawing.Color.Wheat;
            excelSheet.Cells[2, 2] = reportInfo[0];
            excelSheet.get_Range("A2", "B2").Borders.LineStyle = XlLineStyle.xlContinuous;

            //Name
            excelSheet.Cells[4, 1] = "Tester Name";
            excelSheet.get_Range("A4").Interior.Color = System.Drawing.Color.Wheat;
            excelSheet.Cells[4, 2] = reportInfo[1];
            excelSheet.get_Range("A4", "B4").Borders.LineStyle = XlLineStyle.xlContinuous;

            //StartTime
            excelSheet.Cells[2, 4] = "Start Time";
            excelSheet.get_Range("D2").Interior.Color = System.Drawing.Color.Wheat;
            excelSheet.Cells[2, 5] = reportInfo[2];
            excelSheet.get_Range("D2", "E2").Borders.LineStyle = XlLineStyle.xlContinuous;

            //EndTime
            excelSheet.Cells[4, 4] = "End Time";
            excelSheet.get_Range("D4").Interior.Color = System.Drawing.Color.Wheat;
            excelSheet.Cells[4, 5] = reportInfo[3];
            excelSheet.get_Range("D4", "E4").Borders.LineStyle = XlLineStyle.xlContinuous;

            //Note
            //excelSheet.Cells[2, 7] = "Not";
            //excelSheet.get_Range("G2").Interior.Color = System.Drawing.Color.Wheat;
            //excelSheet.get_Range("G2").Borders.LineStyle = XlLineStyle.xlContinuous;


            //excelSheet.get_Range("H2", "J5").Merge();
            //excelSheet.Cells[2, 8] = reportInfo[4];
            //excelSheet.get_Range("H2").HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignDistributed;
            //excelSheet.get_Range("H2", "J5").Borders.LineStyle = XlLineStyle.xlContinuous;

            excelSheet.Cells[6, 1] = "Note";
            excelSheet.get_Range("A6").Interior.Color = System.Drawing.Color.Wheat;
            excelSheet.get_Range("A6").Borders.LineStyle = XlLineStyle.xlContinuous;


            excelSheet.get_Range("B6", "E7").Merge();
            excelSheet.Cells[6, 2] = reportInfo[4];
            excelSheet.get_Range("B6").HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignDistributed;
            excelSheet.get_Range("B6", "E7").Borders.LineStyle = XlLineStyle.xlContinuous;

        }

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        private bool toExcel()
        {

            try
            {
                int columnCount = 0;
                char[] letter = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };

                excelSheet = new Microsoft.Office.Interop.Excel.Application();
                excelSheet.Application.Workbooks.Add(true);


                ExcelHead();


                int columnIndex = 0;
                //Columns Heading of Datagridview     
                foreach (DataGridViewColumn column in m_dataGridView.Columns)
                {
                    if (column.Visible)
                    {
                        columnIndex++;
                        excelSheet.Cells[9, columnIndex] = column.HeaderText;
                    }
                }

                columnCount = columnIndex;
                excelSheet.get_Range("A9", (letter[columnCount - 1] + "9")).Borders.LineStyle = XlLineStyle.xlDouble;
                excelSheet.get_Range("A9", (letter[columnCount - 1] + "9")).Borders[XlBordersIndex.xlDiagonalDown].LineStyle = XlLineStyle.xlLineStyleNone;
                excelSheet.get_Range("A9", (letter[columnCount - 1] + "9")).Borders[XlBordersIndex.xlDiagonalUp].LineStyle = XlLineStyle.xlLineStyleNone;
                excelSheet.get_Range("A9", (letter[columnCount - 1] + "9")).Interior.Color = System.Drawing.Color.SkyBlue;

                int rowIndex = 8;
                //get all rows by column wise 
                foreach (DataGridViewRow row in m_dataGridView.Rows)
                {
                    rowIndex++;
                    columnIndex = 0;
                    foreach (DataGridViewColumn column in m_dataGridView.Columns)
                    {
                        if (column.Visible)
                        {
                            columnIndex++;
                            excelSheet.Cells[rowIndex + 1, columnIndex] = row.Cells[column.Name].FormattedValue;

                        }
                    }
                }

                excelSheet.Visible = true;
                Worksheet workSheet = (Worksheet)excelSheet.ActiveSheet;

                foreach (Worksheet wrkst in excelSheet.Worksheets)
                {
                    Range usedrange = wrkst.UsedRange;
                    usedrange.Columns.AutoFit();
                }


                excelSheet.ActiveWindow.DisplayGridlines = true; //Klavuz çizgilerini kaldırır.
                SetForegroundWindow((IntPtr)excelSheet.Application.Hwnd);

                return true;
            }
            catch (Exception ex)
            {
                int hWnd = excelSheet.Application.Hwnd;
                uint processID;

                GetWindowThreadProcessId((IntPtr)hWnd, out processID);
                Process.GetProcessById((int)processID).Kill();

                Console.WriteLine("toExcel:" + ex.Message);
                return false;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        private void BringExcelWindowToFront(int xlApp)
        {
            SetForegroundWindow((IntPtr)xlApp);  // Note Hwnd is declared as int
        }

    }
}

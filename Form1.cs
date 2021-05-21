using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Memory_Policy_Simulator
{
    public partial class Form1 : Form
    {
        Graphics g;
        PictureBox pbPlaceHolder;
        Bitmap bResultImage;

        public Form1()
        {
            InitializeComponent();
            this.pbPlaceHolder = new PictureBox();
            this.bResultImage = new Bitmap(2048, 2048);
            this.pbPlaceHolder.Size = new Size(2048, 2048);
            g = Graphics.FromImage(this.bResultImage);
            pbPlaceHolder.Image = this.bResultImage;
            this.pImage.Controls.Add(this.pbPlaceHolder);
        }

        // 특정 인덱스를 연결 리스트에서 삭제할 수 있게 하는 메소드

        public LinkedListNode<char> RemoveAt(LinkedList<char> queue, int index)
        {
            LinkedListNode<char> currentNode = queue.First;
            for (int i = 0; i <= index && currentNode != null; i++)
            {
                if (i != index)
                {
                    currentNode = currentNode.Next;
                    continue;
                }

                queue.Remove(currentNode);
                return currentNode;
            }
            throw new IndexOutOfRangeException();
        }

        private void DrawBase(Core core, int windowSize, int dataLength)
        {
            /* parse window */
            var psudoQueue = new LinkedList<char>();

            g.Clear(Color.Black);

            for ( int i = 0; i < dataLength; i++ ) // length
            {
                int psudoCursor = core.pageHistory[i].loc;
                char data = core.pageHistory[i].data;
                int psudoTemp = core.psudoTempII[i].refCount;
                Page.STATUS status = core.pageHistory[i].status;

                switch ( status )
                {
                    case Page.STATUS.PAGEFAULT:
                        psudoQueue.AddLast(data);
                        break;
                    case Page.STATUS.MIGRATION:
                        psudoQueue.RemoveFirst();
                        psudoQueue.AddLast(data);
                        break;
                    case Page.STATUS.MIGRATIONII:
                        if (psudoTemp > 0 && psudoTemp < Int32.Parse(tbWindowSize.Text))
                            RemoveAt(psudoQueue, psudoTemp);
                        psudoQueue.AddLast(data);
                        break;
                }

                for ( int j = 0; j <= windowSize; j++) // height - STEP
                {
                    if (j == 0)
                    {
                        DrawGridText(i, j, data);
                    }
                    else
                    {
                        DrawGrid(i, j);
                    }
                }

                DrawGridHighlight(i, psudoCursor, status);
                int depth = 1;

                foreach ( char t in psudoQueue )
                {
                    DrawGridText(i, depth++, t);
                }
            }
        }


        private void DrawGrid(int x, int y)
        {
            int gridSize = 30;
            int gridSpace = 5;
            int gridBaseX = x * gridSize;
            int gridBaseY = y * gridSize;

            g.DrawRectangle(new Pen(Color.White), new Rectangle(
                gridBaseX + (x * gridSpace),
                gridBaseY,
                gridSize,
                gridSize
                ));
        }

        private void DrawGridHighlight(int x, int y, Page.STATUS status)
        {
            int gridSize = 30;
            int gridSpace = 5;
            int gridBaseX = x * gridSize;
            int gridBaseY = y * gridSize;

            SolidBrush highlighter = new SolidBrush(Color.LimeGreen);

            switch (status)
            {
                case Page.STATUS.HIT:
                    break;
                case Page.STATUS.MIGRATION:
                    highlighter.Color = Color.Purple;
                    break;
                case Page.STATUS.MIGRATIONII:
                    highlighter.Color = Color.Purple;
                    break;
                case Page.STATUS.PAGEFAULT:
                    highlighter.Color = Color.Red;
                    break;
            }

            g.FillRectangle(highlighter, new Rectangle(
                gridBaseX + (x * gridSpace),
                gridBaseY,
                gridSize,
                gridSize
                ));
        }

        private void DrawGridText(int x, int y, char value)
        {
            int gridSize = 30;
            int gridSpace = 5;
            int gridBaseX = x * gridSize;
            int gridBaseY = y * gridSize;

            g.DrawString(
                value.ToString(), 
                new Font("맑은 고딕", 10), 
                new SolidBrush(Color.White), 
                new PointF(
                    gridBaseX + (x * gridSpace) + gridSize / 3,
                    gridBaseY + gridSize / 4));
        }

        private void btnOperate_Click(object sender, EventArgs e)
        {
            this.tbConsole.Clear();

            if (this.tbQueryString.Text != "" || this.tbWindowSize.Text != "")
            {
                string data = this.tbQueryString.Text; // 문자열 변수 data에 쿼리 스트링값 전달
                if (data.Length == 0)
                    MessageBox.Show("스트링을 입력해 주십시오.", "Input Error");

                int windowSize = int.Parse(this.tbWindowSize.Text);

                /* initalize */
                var window = new Core(windowSize);

                if (comboBox1.SelectedItem.Equals("FIFO"))
                {
                    // FIFO : data의 값을 char 형으로 한개 문자씩
                    foreach (char element in data)
                    {
                        var status = window.FIFO(element);
                        this.tbConsole.Text += "DATA " + element + " is " +
                            ((status == Page.STATUS.PAGEFAULT) ? "Page Fault" : status == Page.STATUS.MIGRATION ? "Migrated" : "Hit")
                            + "\r\n";
                        this.tbConsole.Select(tbConsole.Text.Length, 0);
                        this.tbConsole.ScrollToCaret();
                    }
                }

                else if (comboBox1.SelectedItem.Equals("Optimal"))
                {
                    int i = 0;
                    int n = 0;
                    char[] dataArr = new char[data.Length];

                    // Optimal : data의 값을 char 형으로 한개 문자씩 dataArr에 저장
                    foreach (char element in data)
                    {
                        dataArr[i] = element;
                        i++;
                    }

                    foreach (char element in dataArr)
                    {
                        var status = window.Opt(dataArr, n);
                        n++;

                        this.tbConsole.Text += "DATA " + element + " is " +
                            ((status == Page.STATUS.PAGEFAULT) ? "Page Fault" : status == Page.STATUS.MIGRATION ? "Migrated" : status == Page.STATUS.MIGRATIONII ? "Migrated" : "Hit")
                            + "\r\n";

                        this.tbConsole.Select(tbConsole.Text.Length, 0);
                        this.tbConsole.ScrollToCaret();
                    }
                }

                if (comboBox1.SelectedItem.Equals("LRU"))
                {                    
                    // LRU : 가장 오랫동안 사용되지 않은 페이지 교체
                    foreach (char element in data)
                    {
                        var status = window.Lru(element);

                        this.tbConsole.Text += "DATA " + element + " is " +
                            ((status == Page.STATUS.PAGEFAULT) ? "Page Fault" : status == Page.STATUS.MIGRATION ? "Migrated" : status == Page.STATUS.MIGRATIONII ? "Migrated" : "Hit")
                            + "\r\n";

                        this.tbConsole.Select(tbConsole.Text.Length, 0);
                        this.tbConsole.ScrollToCaret();
                    }
                }

                if (comboBox1.SelectedItem.Equals("LFU"))
                {
                    // LFU : 가장 참조 수가 적은 페이지 교체
                    foreach (char element in data)
                    {
                        var status = window.Lfu(element);

                        this.tbConsole.Text += "DATA " + element + " is " +
                            ((status == Page.STATUS.PAGEFAULT) ? "Page Fault" : status == Page.STATUS.MIGRATION ? "Migrated" : status == Page.STATUS.MIGRATIONII ? "Migrated" : "Hit")
                            + "\r\n";

                        this.tbConsole.Select(tbConsole.Text.Length, 0);
                        this.tbConsole.ScrollToCaret();
                    }
                }

                if (comboBox1.SelectedItem.Equals("MFU"))
                {
                    // LRU : 가장 참조 수가 많은 페이지 교체
                    foreach (char element in data)
                    {
                        var status = window.Mfu(element);

                        this.tbConsole.Text += "DATA " + element + " is " +
                            ((status == Page.STATUS.PAGEFAULT) ? "Page Fault" : status == Page.STATUS.MIGRATION ? "Migrated" : status == Page.STATUS.MIGRATIONII ? "Migrated" : "Hit")
                            + "\r\n";

                        this.tbConsole.Select(tbConsole.Text.Length, 0);
                        this.tbConsole.ScrollToCaret();
                    }
                }

                DrawBase(window, windowSize, data.Length);
                this.pbPlaceHolder.Refresh();

                /* 차트 생성 */
                chart1.Series.Clear();
                Series resultChartContent = chart1.Series.Add("Statics");
                resultChartContent.ChartType = SeriesChartType.Pie;
                resultChartContent.IsVisibleInLegend = true;
                resultChartContent.Points.AddXY("Hit", window.hit);
                resultChartContent.Points.AddXY("Page Fault", window.fault-window.migration);
                resultChartContent.Points.AddXY("Migrated", window.migration);
                resultChartContent.Points[0].IsValueShownAsLabel = true;
                resultChartContent.Points[1].IsValueShownAsLabel = true;
                resultChartContent.Points[2].IsValueShownAsLabel = true;

                this.lbPageFaultRatio.Text = Math.Round(((float)window.fault / (window.fault + window.hit)), 2) * 100 + "%";
            }
            else
            {
            }

        }

        private void pbPlaceHolder_Paint(object sender, PaintEventArgs e)
        {
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void tbWindowSize_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void tbWindowSize_KeyPress(object sender, KeyPressEventArgs e)
        {
                if (!(Char.IsDigit(e.KeyChar)) && e.KeyChar != 8)
                {
                    e.Handled = true;
                }
        }

        private void btnRand_Click(object sender, EventArgs e)
        {
            Random rd = new Random();

            int count = rd.Next(5, 50);
            StringBuilder sb = new StringBuilder();


            for ( int i = 0; i < count; i++ )
            {
                sb.Append((char)rd.Next(65, 90));
            }

            this.tbQueryString.Text = sb.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bResultImage.Save("./result.jpg");
        }

        private void tbConsole_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbWindowSize_TextChanged(object sender, EventArgs e)
        {

        }

        private void pImage_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tbQueryString_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

namespace Lottery
{
    public partial class frmSSQ : Form
    {
        /// <summary>
        /// 多執行序樂透規則
        /// 紅色號碼1~40 (不重複)
        /// 藍色號碼1~16
        /// </summary>
        public frmSSQ()
        {
            InitializeComponent();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            
        }

        #region data
        /// <summary>
        /// 紅球集合
        /// </summary>
        private string[] RedNum =
        {
            "01","02","03","04","05","06","07","08","09","10",
            "11","12","13","14","15","16","17","18","19","20",
            "21","22","23","24","25","26","27","28","29","30",
            "31","32","33","34","35","36","37","38","39","40"
        };

        /// <summary>
        /// 籃球集合
        /// </summary>
        private string[] BlueNum =
        {
            "01","02","03","04","05","06","07","08","09","10",
            "11","12","13","14","15","16"
        };
        #endregion
        
        private static readonly object frmSSQ_Lock = new object();
        private bool IsGoOn = true;
        private List<Task> taskList = new List<Task>();

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                this.btnStart.Text = "運行中";
                this.btnStart.Enabled = false;
                this.IsGoOn = true;
                this.taskList = new List<Task>();
                this.lblBlue.Text = "00";
                this.lblRed1.Text = "00";
                this.lblRed2.Text = "00";
                this.lblRed3.Text = "00";
                this.lblRed4.Text = "00";
                this.lblRed5.Text = "00";
                this.lblRed6.Text = "00";

                Thread.Sleep(1000);
                TaskFactory taskFactory = Task.Factory;
                foreach (var control in this.gboSSQ.Controls)
                {
                    if (control is Label)
                    {
                        Label lbl = (Label)control;
                        if (lbl.Name.Contains("Blue"))
                        {
                            taskList.Add(taskFactory.StartNew(() =>
                            {
                                while (IsGoOn)
                                {
                                    int indexNum = new RandomHelper().GetNumber(0, this.BlueNum.Length);
                                    string sNumber = this.BlueNum[indexNum];
                                    UpdateLbl(lbl, sNumber);
                                }
                            }));
                        }
                        else//紅球
                        {
                            taskList.Add(taskFactory.StartNew(() =>
                            {
                                while (IsGoOn)
                                {
                                    //不用內建亂數，內建亂數其實不是真正的亂數
                                    int indexNum = new RandomHelper().GetNumber(0, this.RedNum.Length);
                                    string sNumber = this.RedNum[indexNum];
                                    //鎖需要包住判斷與更新動作
                                    lock (frmSSQ_Lock)
                                    {
                                        //判斷現在所有球是否有同數字，但有可能在毫秒內同時更新的數字都是同樣的，所以需要加鎖來防止同時判斷
                                        if (this.IsExistRed(sNumber))
                                        {
                                            continue;//重複了就放棄更新，重新取得
                                        }
                                        UpdateLbl(lbl, sNumber);
                                    }
                                }
                            }));
                        }
                    }
                    //正確啟動後，打開stop按鈕
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(1000);
                            if (!this.IsExistRed("00") && !this.lblBlue.Text.Equals("00"))
                            {
                                //需要控制UI界面都要加入這段
                                this.Invoke(new Action(() => {
                                    btnStop.Enabled = true;
                                }));                                
                                break;
                            }
                        }
                    });

                }

                //全部執行序執行完成顯示結果
                taskFactory.ContinueWhenAll(this.taskList.ToArray(), tArray => this.ShowResult());
            }
            catch (Exception ex)
            {
                Console.WriteLine("樂透系統出現異常 : {0}",ex.ToString());
            }
        }

        /// <summary>
        /// 判斷是否為紅球的某個數字
        /// </summary>
        /// <param name="sNumber"></param>
        /// <returns></returns>
        private bool IsExistRed(string sNumber)
        {
            foreach (var control in this.gboSSQ.Controls)
            {
                if (control is Label)
                {
                    Label lbl = (Label)control;
                    if (lbl.Name.Contains("Red")&&lbl.Text.Equals(sNumber))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ShowResult()
        {
            MessageBox.Show(string.Format("本期樂透結果為: {0} {1} {2} {3} {4} {5} 藍球 {6}"
                , this.lblRed1.Text
                , this.lblRed2.Text
                , this.lblRed3.Text
                , this.lblRed4.Text
                , this.lblRed5.Text
                , this.lblRed6.Text
                , this.lblBlue.Text));
        }
        /// <summary>
        /// 要操作UI主執行序控件(更新介面)
        /// </summary>
        /// <param name="lbl"></param>
        /// <param name="text"></param>
        private void UpdateLbl(Label lbl, string text)
        {
            if (lbl.InvokeRequired)
            {
                this.Invoke(new Action(() => {
                    lbl.Text = text;
                }));//交給UI線程去更新
            }
            else
            {
                lbl.Text = text;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.btnStart.Enabled = true;
            this.btnStart.Text = "Start";
            this.btnStop.Enabled = false;
            this.IsGoOn = false;
        }
    }
}
namespace Lottery
{
    public partial class frmSSQ : Form
    {
        /// <summary>
        /// �h����Ǽֳz�W�h
        /// ���⸹�X1~40 (������)
        /// �Ŧ⸹�X1~16
        /// </summary>
        public frmSSQ()
        {
            InitializeComponent();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            
        }

        #region data
        /// <summary>
        /// ���y���X
        /// </summary>
        private string[] RedNum =
        {
            "01","02","03","04","05","06","07","08","09","10",
            "11","12","13","14","15","16","17","18","19","20",
            "21","22","23","24","25","26","27","28","29","30",
            "31","32","33","34","35","36","37","38","39","40"
        };

        /// <summary>
        /// �x�y���X
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
                this.btnStart.Text = "�B�椤";
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
                        else//���y
                        {
                            taskList.Add(taskFactory.StartNew(() =>
                            {
                                while (IsGoOn)
                                {
                                    //���Τ��ضüơA���ضüƨ�ꤣ�O�u�����ü�
                                    int indexNum = new RandomHelper().GetNumber(0, this.RedNum.Length);
                                    string sNumber = this.RedNum[indexNum];
                                    //��ݭn�]��P�_�P��s�ʧ@
                                    lock (frmSSQ_Lock)
                                    {
                                        //�P�_�{�b�Ҧ��y�O�_���P�Ʀr�A�����i��b�@���P�ɧ�s���Ʀr���O�P�˪��A�ҥH�ݭn�[��Ө���P�ɧP�_
                                        if (this.IsExistRed(sNumber))
                                        {
                                            continue;//���ƤF�N����s�A���s���o
                                        }
                                        UpdateLbl(lbl, sNumber);
                                    }
                                }
                            }));
                        }
                    }
                    //���T�Ұʫ�A���}stop���s
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(1000);
                            if (!this.IsExistRed("00") && !this.lblBlue.Text.Equals("00"))
                            {
                                //�ݭn����UI�ɭ����n�[�J�o�q
                                this.Invoke(new Action(() => {
                                    btnStop.Enabled = true;
                                }));                                
                                break;
                            }
                        }
                    });

                }

                //��������ǰ��槹����ܵ��G
                taskFactory.ContinueWhenAll(this.taskList.ToArray(), tArray => this.ShowResult());
            }
            catch (Exception ex)
            {
                Console.WriteLine("�ֳz�t�ΥX�{���` : {0}",ex.ToString());
            }
        }

        /// <summary>
        /// �P�_�O�_�����y���Y�ӼƦr
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
            MessageBox.Show(string.Format("�����ֳz���G��: {0} {1} {2} {3} {4} {5} �Ųy {6}"
                , this.lblRed1.Text
                , this.lblRed2.Text
                , this.lblRed3.Text
                , this.lblRed4.Text
                , this.lblRed5.Text
                , this.lblRed6.Text
                , this.lblBlue.Text));
        }
        /// <summary>
        /// �n�ާ@UI�D����Ǳ���(��s����)
        /// </summary>
        /// <param name="lbl"></param>
        /// <param name="text"></param>
        private void UpdateLbl(Label lbl, string text)
        {
            if (lbl.InvokeRequired)
            {
                this.Invoke(new Action(() => {
                    lbl.Text = text;
                }));//�浹UI�u�{�h��s
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
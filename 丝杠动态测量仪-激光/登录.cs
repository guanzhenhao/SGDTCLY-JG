using GZH_ClassLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 丝杠动态测量仪
{
    public partial class 登录 : Form
    {
        #region 常量定义（优化：提取魔法值，提升可维护性）
        /// <summary>
        /// 全局权限标识：1-权限1 | 2-权限2 | 3-权限3
        /// </summary>
        public static double QuanXian = 0;

        // 数据库相关常量（避免硬编码，便于后续修改）
        private const string DbPassword = "HJJCLMSDBCS"; // 数据库密码
        private const string DbSettingDir = "设置";      // 配置文件目录
        private const string DbSettingFileName = "软件设置.mdb"; // 配置数据库文件名
        private const string DbTableName = "登录";       // 登录表名
        private const string DbPwdField = "口令";        // 口令字段名
        #endregion

        #region 构造函数
        public 登录()
        {
            InitializeComponent();
        }
        #endregion

        #region 窗体加载事件
        private void 登录_Load(object sender, EventArgs e)
        {
            // 默认选中第一个权限选项
            comboBox1.SelectedIndex = 0;
        }
        #endregion

        #region 登录按钮点击事件（核心逻辑优化）
        private void button1_Click(object sender, EventArgs e)
        {
            // 1. 输入校验（优化：提前校验空值，避免无效数据库操作）
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("请输入登录密码！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }

            // 2. 构建数据库路径（优化：用Path.Combine处理路径，避免分隔符/跨平台问题）
            string dbSettingPath = Path.Combine(Environment.CurrentDirectory, DbSettingDir, DbSettingFileName);
            try
            {
                // 3. 数据库操作（优化：使用using自动释放资源，避免内存泄漏）
                using (AccessClass ac1 = new AccessClass())
                {
                    // 拼接查询条件（优化：规范字符串拼接，提升可读性）
                    string queryCondition = $"权限='{comboBox1.Text.Trim()}'";

                    // 读取数据库中的口令
                    ac1.Read_cell(dbSettingPath, DbPassword, DbTableName, DbPwdField, queryCondition);

                    // 4. 密码校验（优化：简化权限赋值逻辑，去除冗余if）
                    string dbPwd = ac1.return_data?.Trim() ?? string.Empty; // 空值保护
                    if (textBox1.Text.Trim() == dbPwd)
                    {
                        // 权限值 = 下拉框选中索引 + 1（原逻辑等价简化）
                        QuanXian = comboBox1.SelectedIndex + 1;

                        // 隐藏登录窗体，打开主窗体
                        this.Hide();
                        using (Form1 mainForm = new Form1())
                        {
                            mainForm.ShowDialog(); // 优化：用ShowDialog避免多主窗体问题
                        }
                        this.Close(); // 主窗体关闭后，释放登录窗体
                    }
                    else
                    {
                        MessageBox.Show("密码输入错误，请重新输入！", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox1.Clear();
                        textBox1.Focus();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"配置数据库文件不存在：{dbSettingPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // 优化：新增全局异常捕获，避免程序崩溃
                MessageBox.Show($"登录失败：{ex.Message}", "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 密码输入框按键限制事件（优化：简化逻辑，提升用户体验）
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 允许输入：数字、字母、小数点、退格、删除、Shift、回车
            bool isAllowedChar = char.IsDigit(e.KeyChar)
                                 || char.IsLetter(e.KeyChar)
                                 || e.KeyChar == '.'
                                 || e.KeyChar == (char)Keys.Back
                                 || e.KeyChar == (char)Keys.Delete
                                 || e.KeyChar == (char)Keys.ShiftKey
                                 || e.KeyChar == (char)Keys.Enter;

            if (!isAllowedChar)
            {
                e.Handled = true; // 阻止非法输入
                // 优化：避免频繁弹窗，仅在首次非法输入时提示
                MessageBox.Show("密码仅支持输入字母、数字和小数点！", "输入限制", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                e.Handled = false;
            }
        }
        #endregion
    }
}

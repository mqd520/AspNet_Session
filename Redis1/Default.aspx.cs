using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Redis1
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text.Trim() != "")
            {
                ShowError(false);
                Session[TextBox1.Text.Trim()] = TextBox2.Text.Trim();
            }
            else
            {
                ShowError(true, "key is empty!");
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text.Trim() != "")
            {
                if (Session[TextBox1.Text.Trim()] == null)
                {
                    ShowError(true, "key not exist!");
                }
                else
                {
                    ShowError(false);
                    TextBox2.Text = Session[TextBox1.Text.Trim()].ToString();
                }
            }
            else
            {
                ShowError(true, "key is empty!");
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text.Trim() != "")
            {
                Session[TextBox1.Text.Trim()] = null;
            }
            else
            {
                ShowError(true, "key is empty!");
            }
        }

        protected void ShowError(bool show, string msg = "")
        {
            if (show)
            {
                lb_error.Visible = true;
                lb_error.Text = msg;
            }
            else
            {
                lb_error.Visible = false;
            }
        }

        protected void Button4_Click(object sender, EventArgs e)
        {
            TextBox1.Text = "";
            TextBox2.Text = "";
        }
    }
}
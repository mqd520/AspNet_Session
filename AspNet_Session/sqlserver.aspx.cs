using System;
using System.Collections.Generic;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AspNet_Session
{
    public partial class sqlserver : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text.Trim() != "")
            {
                Session[TextBox1.Text.Trim()] = TextBox2.Text.Trim();
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text.Trim() != "")
            {
                if (Session[TextBox1.Text.Trim()] == null)
                {
                    TextBox2.Text = "null";
                }
                else
                {
                    TextBox2.Text = Session[TextBox1.Text.Trim()].ToString();
                }
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text.Trim() != "")
            {
                Session[TextBox1.Text.Trim()] = null;
            }
        }

        protected void ShowError(bool show,string msg)
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
    }
}
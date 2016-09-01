using System;
using System.Reflection;
using System.Web.UI;

namespace GX26Bot
{
	public partial class _default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				AssemblyName name = assembly.GetName();
				Version ver = name.Version;

				string strVersion = $"R.U.D.I. GX26 v{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
				Page.Title = strVersion;
				lblVersion.Text = strVersion;
			}
		}
	}
}
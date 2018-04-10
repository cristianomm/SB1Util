using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Windows.Forms;
using SB1Util.Log;



namespace SB1Util.Report
{
    public class Reports
    {
        private Logger logger;


        public Reports()
        {
            this.logger = Logger.getInstance();

        }


        public bool openReport(string reportFile)
        {
            bool ret = false;
            logger.pushOperation("Reports.loadReport");
            try
            {
                CrystalReportViewer crv = new CrystalReportViewer();
                ReportDocument report = new ReportDocument();

                report.Load(reportFile);

                //report.ParameterFields
                crv.ReportSource = report;

            }
            catch (Exception e)
            {
                ret = false;
                logger.log("Erro: " + e.Message, Logger.LogType.ERROR, e, false);
            }
            logger.releaseOperation();
            return ret;
        }



    }
}

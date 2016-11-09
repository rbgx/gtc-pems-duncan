﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

using System.Web.Mvc.Html;
using System.Linq.Expressions;

using System.Web.Mvc;


using Duncan.PEMS.SpaceStatus.DataShapes;
using Duncan.PEMS.SpaceStatus.DataSuppliers;
using Duncan.PEMS.SpaceStatus.Models;

namespace Duncan.PEMS.SpaceStatus.Helpers
{
    public static class HTMLHelper_RegulatedHoursDetail
    {
        public static string GetFormEditDefaultsForDay(RegulatedHoursGroup group, DayOfWeek day, CustomerConfig customerCfg)
        {
            StringBuilder sb = new StringBuilder();

            string delim = string.Empty;

            sb.AppendLine("$(\"#itemDetails_" + day.ToString().Substring(0, 3) + "\").EnableMultiField({");
            sb.AppendLine("   data:[");

            foreach (RegulatedHoursDetail nextDetail in group.Details)
            {
                // Skip if its not for the day we are interested in
                if (nextDetail.DayOfWeek != (int)day)
                    continue;

                if (string.IsNullOrEmpty(delim))
                    delim = ",";
                else
                    sb.AppendLine(delim);

                sb.AppendLine("   {");
                sb.AppendLine("      " + "detail_Type : " + "\"" + nextDetail.Type.ToString() + "\",");
                sb.AppendLine("      " + "detail_StartTime : " + "\"" + nextDetail.StartTime.ToString("hh:mm:ss tt") + "\",");
                sb.AppendLine("      " + "detail_EndTime : " + "\"" + nextDetail.EndTime.ToString("hh:mm:ss tt") + "\",");
                sb.AppendLine("      " + "detail_DayOfWeek : " + "\"" + Convert.ToString((int)nextDetail.DayOfWeek) + "\",");
                sb.AppendLine("      " + "detail_MaxStayMinutes : " + "\"" + nextDetail.MaxStayMinutes.ToString() + "\",");
                sb.AppendLine("      " + "detail_RegulatedHoursDetailID : " + "\"" + nextDetail.RegulatedHoursDetailID.ToString() + "\"");
                sb.Append("   }");
            }
            sb.AppendLine();
            sb.AppendLine("   ],");
            sb.AppendLine("   addEventCallback : function(newElem, clonnedFrom){commonCloneFormFieldsEvent(newElem, clonnedFrom);} });");

            return sb.ToString();
        }
    }


}
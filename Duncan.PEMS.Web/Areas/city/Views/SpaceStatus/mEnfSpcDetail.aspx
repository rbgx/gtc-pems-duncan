﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<List<Duncan.PEMS.SpaceStatus.Models.SpaceStatusModel>>" %>

<%@ Import Namespace="Duncan.PEMS.SpaceStatus.Helpers" %>
<%@ Import Namespace="Duncan.PEMS.SpaceStatus.Models" %>
<%@ Import Namespace="Duncan.PEMS.SpaceStatus.DataShapes" %>
<%@ Import Namespace="System.Linq" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">

<meta http-equiv="cache-control" content="no-cache"/> <%-- <!-- tells browser not to cache --> --%>
<meta http-equiv="expires" content="-1"/> <%-- <!-- says that the cache expires 'now' --> --%>
<meta http-equiv="pragma" content="no-cache"/> <%-- <!-- says not to use cached stuff, if there is any --> --%>

<%-- Meta tags for better viewing on mobile devices, such as the iPhone or Android --%>
<meta name="viewport" content="width=device-width, initial-scale=1" />
<meta name="HandheldFriendly" content="True">
<meta name="MobileOptimized" content="320">

<title>Mobile Enforcement Space Details</title>

<style type="text/css">
#header, #footer2 {text-align:center; background:#f00; color:#fff; font:900 1em arial,sans-serif;}
#content {height:100%; text-align:left; background:#fff; color:#000; font:1em arial,sans-serif; overflow:auto;}
div.side-by-side > div { float: left; padding-left:8px; }
.dtNoLinesOrGaps {display:table; border-collapse:collapse; border-spacing:0px; border-width:0px;}
.dtNoLinesOrGapsBOGUS {display:table; border-collapse:separate; border-spacing:1px; border-width:0px;}
.dtr {display: table-row;}
.dtc {display: table-cell; border-bottom: 1px solid #e8eef4;}
.hcenter {text-align: center;}
.vcenter {vertical-align:middle;}
.LCol {width:102px; padding-bottom:4px; text-align:left;}
.RCol {width:102px; padding-bottom:4px; text-align:right;}
.loading {display: inline-block; left:8px; height: 40px; line-height: 40px; top:58px; width:188px; border:1px solid #000; 
          position:fixed; background-color:#FFFF00; background-color: rgba(255,255,0,.8); color:Black; text-align:center; font-size: 16px;}
* html #loading {position:absolute;}

@media screen and (max-width: 300px) <%-- /* for small mobile devices */ --%>
{
  div.side-by-side { height:400px;  <%-- /*width: 100%;*/ /*margin-bottom: 1em;*/ --%> }
  .clearfix:after {content: "\0020"; display: block; height: 0; clear: both; overflow: hidden; visibility: hidden; }
  .footerarea  { position:absolute; bottom:0px; height:28px; left:0px; overflow:hidden;    border-style:none; border-color:red; }
  .headerarea {position:absolute; top:0px; left:0px; overflow:hidden; color:#004475;}
  .contentarea {position:absolute; top:50px; bottom:28px; left:0px; overflow:auto;   border-style:none; border-color:blue; }
}

@media screen and (min-width: 301px) <%-- /* for larger mobile devices and/or desktop */ --%>
{
  div.side-by-side {  max-height:24px;  <%-- /*width: 100%;*/ /*margin-bottom: 1em;*/ --%> }
  .clearfix2:after {content: "\0020"; display: block; height: 0; clear: both; overflow: hidden; visibility: hidden; }
  .headerarea {display:block; height:50px; overflow:visible;}
  .footerarea  { height:28px; left:0px; overflow:hidden;  border-style:none; border-color:red; }
  .contentarea{ max-width: 230px; max-height:522px; overflow:auto;}
}

</style>

<%-- <!-- Grab Google CDN's jQuery, with a protocol relative URL; fall back to local if offline --> --%>
<script src="//ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js" type="text/javascript"></script>
<script type="text/javascript">    window.jQuery || document.write('<script src="<%= Url.Content("~/Scripts/jquery-1.7.2.min.js") %>"><\/script>')</script>

<link rel="stylesheet" href="<%= Url.Content("~/Scripts/Chosen/chosen.css") %>" type="text/css" />
<script src="<%= Url.Content("~/Scripts/Chosen/chosen.jquery.min.js") %>" type="text/javascript"></script>

</head>

<script type="text/javascript">
    var targetID = "";

    function drilldownToSelected() {
        if (targetID == ""){
          targetID = $("#GroupSelect option:selected").attr('value');
        }
        
        if (targetID != "") {
            stopStatusRefreshTimer();
            $('#loading').css('display', 'inline-block');

            window.location.href = '<%= Url.Action("mEnfSpcDetail","SpaceStatus") %>' + 
              '?T=' + encodeURIComponent(targetID) +  /*encodeURIComponent('<%= this.ViewData["originalTarget"].ToString() %>') + */
              '&PT=' + encodeURIComponent('<%= this.ViewData["parentTarget"].ToString() %>') +
              '&G=' + encodeURIComponent('<%= this.ViewData["groupType"].ToString() %>') +
              '&V=' + encodeURIComponent('<%= this.ViewData["viewType"].ToString() %>') +
              '&CID=' + encodeURIComponent('<%= (this.ViewData["CustomerCfg"] as CustomerConfig).CustomerId.ToString() %>');
        }
    }

    function navigateBackClick(){
      stopStatusRefreshTimer();
      $('#loading').css('display', 'inline-block');
      window.history.back();    
    }

    function refreshCurrentInfo() {
        /*
        $('#loading').css('display', 'inline-block');
        window.location.reload(true);
        */

        stopStatusRefreshTimer();
        <%-- /*Make the "updating" element visible. Here is alternatate sytax: $('#UpdatingBlock').css({ "visibility": "visible" }); */ --%>
        $('#loading').css('display', 'inline-block');
        if (targetID == ""){
          targetID = $("#GroupSelect option:selected").attr('value');
        }
        $('#partial1').load('<%= Url.Action("mEnfSpcDetailPartial","SpaceStatus") %>' + 
          '?T=' + encodeURIComponent('<%= this.ViewData["originalTarget"].ToString() %>') + 
          '&PT=' + encodeURIComponent('<%= this.ViewData["parentTarget"].ToString() %>') +
          '&G=' + encodeURIComponent('<%= ViewData["groupType"]%>') + 
          '&V=' + encodeURIComponent('<%= ViewData["viewType"]%>') + 
          '&CID=' + encodeURIComponent('<%= (this.ViewData["CustomerCfg"] as CustomerConfig).CustomerId.ToString() %>'));
    }
</script>

<body style="margin:0px; padding:0px; font-family: helvetica,arial,sans-serif; font-size: 14px; font-weight: 700;" >

<div id="container">

<div id="myheader" class="headerarea">
  <div style="font-style:italic; width:204px; overflow:hidden; max-height:15px; text-align:center; font-size: 14px; color:Blue; padding-bottom:4px;">Space Enforcement Details</div>

  <%-- Don't render this section unless we have more than 1 item to choose from --%>
  <% if ((this.ViewData["groupChoices"] as SelectList).Count() > 1) %>
  <% { %>
  <div  class="side-by-side clearfix" style="height:400px; border-style:none; overflow:visible;" >
   <div>Space:</div>
   <div>
    <%= Html.DropDownList("GroupSelect", ViewData["groupChoices"] as SelectList, new { @class = "chzn-select", style = "min-width:100px;" })%>
    <script type="text/javascript">        $(".chzn-select").chosen(); $(".chzn-select-deselect").chosen({ allow_single_deselect: true }); </script>
    <%-- This is at a space-detail level, so there are no other spaces that can be scrolled to on this page, but we still need to retain selection for drill-down purposes! --%>
    <script type="text/javascript">
        $("#GroupSelect").chosen().change(function () {
            targetID = $("#GroupSelect option:selected").val();
    <%-- 
            document.getElementById("m" + $("#GroupSelect option:selected").val()).scrollIntoView();
    --%>
        });
     </script>
   </div>

  <div>
   <input type="button" value="&nbsp;" style="width:24px; height:24px; border-style:none; background-image: url(<%= Url.Content("~/content/images/arrow-right-blue.gif") %>);
   background-repeat: no-repeat; background-position:0 0;" onclick="drilldownToSelected();"  />
  </div>

  </div>
  <% } %>

</div>

<div id="mycontent" class="contentarea" >

  <div id="loading" class="loading" style="display:none;">LOADING...</div>

<script type="text/javascript">
    var intervalStatusRefresh = "";
             
    

    function stopStatusRefreshTimer() {
        if (intervalStatusRefresh != "") {
            window.clearInterval(intervalStatusRefresh);
            intervalStatusRefresh = "";
        }
    }
</script>

<span id="partial1">
  <% Html.RenderPartial("pv_mEnfSpcDetailContent"); %>
</span>

  <%-- <%= MobileOccupancyStatusHelpers.EnfSpaceDetails(this.Model, this.ViewData)%> --%>

  <%-- This hidden input and "onload" event will help reload the page when navigating back to it (instead of using browser's cached version) --%>
  <input type="hidden" id="refreshed" value="no">
  <script type="text/javascript">
      onload = function () {
          var e = document.getElementById("refreshed");
          if (e.value == "no") 
          { 
            e.value = "yes"; 
          }
          else 
          { 
            e.value = "no"; 
            /* Page is being displayed from browser cache, so show the "Loading" indictor and force a reload of the page */
            $('#loading').css('display', 'inline-block');

  <%-- Don't include this in the javascript unless we have more than 1 item to choose from --%>
  <% if ((this.ViewData["groupChoices"] as SelectList).Count() > 1) %>
  <% { %>
            $("#GroupSelect").prop("selectedIndex", <%=ViewData["SelectedIndex"]%>);
  <% } %>

            location.reload(); 
          }

  <%-- Don't include this in the javascript unless we have more than 1 item to choose from --%>
  <% if ((this.ViewData["groupChoices"] as SelectList).Count() > 1) %>
  <% { %>
          /* Force the correct selected item to actually be selected. Sometimes the browser just doesn't set the correct one that is declared in HTML... */
          $("#GroupSelect").prop("selectedIndex", <%=ViewData["SelectedIndex"]%>);
  <% } %>
      }

      /* "onunload" event is a fix for Firefox Backward-Forward Cache. See http://stackoverflow.com/questions/2638292/after-travelling-back-in-firefox-history-javascript-wont-run for info */
      window.onunload = function () { };
  </script>

</div>

<div id="myfooter"  class="footerarea">
   <input type="button" class="treeActionBtnBOGUS" value="&nbsp;" style="width:43px; height:24px; border-style:none; background-image: url(<%= Url.Content("~/content/images/arrow-left-blue.gif") %>);
   background-repeat: no-repeat; background-position:center center; background-color:#004475;" onclick="navigateBackClick();"  />

   <input type="button" class="treeActionBtnBOGUS" value="&nbsp;" style="visibility:hidden; width:43px; height:24px; border-style:none; background-image: url(<%= Url.Content("~/content/images/zoom-btn-blue.png") %>);
   background-repeat: no-repeat; background-position:center center; background-color:#004475;"  />

   <input type="button" class="treeActionBtnBOGUS" value="&nbsp;" style="visibility:hidden; width:43px; height:24px; border-style:none; background-image: url(<%= Url.Content("~/content/images/sort-btn-blue.gif") %>);
   background-repeat: no-repeat; background-position:center center; background-color:#004475;"  />

   <input type="button" class="treeActionBtnBOGUS" value="&nbsp;" style="width:43px; height:24px; border-style:none; background-image: url(<%= Url.Content("~/content/images/arrow-refresh-for-blue.png") %>);
   background-repeat: no-repeat; background-position:center center; background-color:#004475;" onclick="refreshCurrentInfo();"  />
</div>

</div>

</body>
</html>


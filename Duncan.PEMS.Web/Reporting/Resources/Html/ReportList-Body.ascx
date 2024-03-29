<%@ Control Language="C#" AutoEventWireup="true" %>

	<div id="cursorChange" class="report-list-page">

		<table class="layout-table" cellpadding="0" cellspacing="0">
			<tr>
				<td class="blue-panel" style="width: 250px;">
					<div class="left-panel">

						<div class="search-panel">
							<input id="RL_QuickSearchBox" type="text" onblur="RL_BlurSearch(this);" onfocus="RL_FocusSearch(this);" onkeyup="RL_SearchInputStartTimeout(this.value.toLowerCase());" value="Search" />
							<a id="RL_CancelSearchIcon" title="Cancel search" onclick="var sb = document.getElementById('RL_QuickSearchBox'); sb.value = ''; searchKeyword = ''; RL_BlurSearch(sb); RL_SearchReports();">&times;</a>
							<span class="search-icon"></span>
							<img id="RL_SearchingIcon" title="Searching..." alt="Searching..." style="height: 16px; width: 16px; border-width: 0px; position: relative; top: 3px; display: none;" src="rs.aspx?image=searching-icon.gif" />
						</div>

						<h2>Categories</h2>
						<div id="leftSideCats" style="white-space: nowrap;">
						</div>

						<h2>Recent</h2>
						<div id="recentReports">
						</div>

					</div>
				</td>

				<td class="content-panel" id="contentDiv">
					<div class="right-panel">
						<div id="loadingDiv" style="width: 100%; text-align: center;">
						  <div id="loadingWord" style="font-size: 20px;color:#1D5987;font-family:Verdana,Arial,Helvetica,sans-serif;font-weight:normal !important;font-size: 20px; font-style: normal;">Loading...</div>
							<img style="padding-top: 40px;" id="loadingImg" alt="" src='rs.aspx?image=loading.gif' />
						</div>
						<div id="reportListDiv" style="visibility: hidden;">
						</div>

						<div id="addInstantReportContainerDiv" class="ui-tabs-panel ui-widget-content ui-corner-bottom" style="display: none;">
							<div id="irdivlink" class="thumb">
								<div class="thumb-container light-colored-text report-list-icon">
									<div style="text-align: center; width: 100%; top: 50%; position: absolute; margin-top: -14px;">New report</div>
								</div>
							</div>
						</div>
					</div>
				</td>
			</tr>
		</table>

	</div>

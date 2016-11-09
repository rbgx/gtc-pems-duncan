﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<Duncan.PEMS.SpaceStatus.Models.SpaceStatusModel>>" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Duncan.PEMS.SpaceStatus.Helpers" %>
<%@ Import Namespace="Duncan.PEMS.SpaceStatus.Models" %>
<%@ Import Namespace="Duncan.PEMS.SpaceStatus.DataShapes" %>

<%= SpaceStatusHelpers.StayVioSpaceDetails(Url, this.Model, this.ViewData)%>

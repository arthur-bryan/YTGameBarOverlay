﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace YoutubeGameBarWidget.Pages
{
    /// <summary>
    /// The resources for the History Page.
    /// Its values are intended to be set with the content on the user language's XML and with user preferences.
    /// </summary>
    [XmlRoot("HistoryPageResources")]
    public class HistoryPageResources
    {
        [XmlElement("Title")]
        public string Title { get; set; }
    }
}

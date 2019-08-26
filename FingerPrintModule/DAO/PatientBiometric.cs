﻿using System;
using System.Collections.Generic;

namespace FingerPrintModule.DAO
{
    public class FingerPrintInfo
    {
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public int ImageDPI { get; set; }
        public int ImageQuality { get; set; }
        public string Image { get; set; }
        public byte[] ImageByte { get; set; }
        public string Template { get; set; }
        public FingerPositions FingerPositions { get; set; }
        public string staffId { get; set; }
        public DateTime DateCreated {get;set;}
        public int Creator { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class SaveModel
    {
       public List<FingerPrintInfo> FingerPrintList { get; set; }
        public string StaffId { get; set; }
    }

    public class FingerPrintMatchInputModel
    {
        public string FingerPrintTemplate { get; set; }

        public List<FingerPrintInfo> FingerPrintTemplateListToMatch { get; set; }
    }

    public class ResponseModel
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }


    public class ConnectionString
    { 
        public string Server { get; set; }
        public string Port { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullConnectionString
        {
            get
            {
                return $"Server={Server};port={Port};Database={DatabaseName};Uid={Username};Pwd={Password};";
                    
            }
        }
        public bool IsOpenMRSDB { get; set; }
    }


    public enum FingerPositions
    {
        RightThumb = 1,
        RightIndex = 2,
        RightMiddle = 3,
        RightWedding = 4,
        RightSmall = 5,
        LeftThumb = 6,
        LeftIndex = 7,
        LeftMiddle = 8,
        LeftWedding = 9,
        LeftSmall = 10
    }
}

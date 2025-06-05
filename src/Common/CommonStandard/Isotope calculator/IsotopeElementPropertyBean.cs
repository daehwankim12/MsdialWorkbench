using MessagePack; // Added
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rfx.Riken.OsakaUniv
{
    /// <summary>
    /// 'IsotopeElementPropertyBean.cs' and 'CompoundPropertyBean.cs' are the storage of isotope calculation result for a fomula.
    /// The detail of the properties such as M+1, M+2, etc is stored in this class.
    /// 
    /// Old version of IsotopicPeak.cs of Common assembly. (Just class name was changed.)
    /// This class is used in MS-DIAL though. MS-Finder program is useing new version.
    /// </summary>
    [Serializable()]
    [MessagePackObject] // Added
    public class IsotopeElementPropertyBean
    {
        private double relativeAbundance;
        private double massDifferenceFromMonoisotopicIon;
        private string comment;

        [Key(0)] // Added
        public double RelativeAbundance
        {
            get { return relativeAbundance; }
            set { relativeAbundance = value; }
        }

        [Key(1)] // Added
        public double MassDifferenceFromMonoisotopicIon
        {
            get { return massDifferenceFromMonoisotopicIon; }
            set { massDifferenceFromMonoisotopicIon = value; }
        }

        [Key(2)] // Added
        public string Comment
        {
            get { return comment; }
            set { comment = value; }
        }
    }
}

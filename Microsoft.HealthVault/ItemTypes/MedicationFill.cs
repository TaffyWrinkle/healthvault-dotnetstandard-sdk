// Copyright (c) Microsoft Corporation.  All rights reserved.
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using Microsoft.HealthVault.Clients;
using Microsoft.HealthVault.Exceptions;
using Microsoft.HealthVault.Helpers;
using Microsoft.HealthVault.Thing;

namespace Microsoft.HealthVault.ItemTypes
{
    /// <summary>
    /// Represents a thing about filling a medication.
    /// </summary>
    ///
    public class MedicationFill : ThingBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MedicationFill"/> class with default
        /// values.
        /// </summary>
        ///
        /// <remarks>
        /// The item is not added to the health record until the <see cref="IThingClient.CreateNewThingsAsync{ThingBase}(Guid, ICollection{ThingBase})"/> method is called.
        /// </remarks>
        ///
        public MedicationFill()
            : base(TypeId)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MedicationFill"/> class with the specified name.
        /// </summary>
        ///
        /// <param name="name">
        /// The name of the medication.
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <b>null</b>.
        /// </exception>
        ///
        public MedicationFill(CodableValue name)
            : base(TypeId)
        {
            Name = name;
        }

        /// <summary>
        /// The unique identifier for the item type.
        /// </summary>
        ///
        public static new readonly Guid TypeId =
            new Guid("167ecf6b-bb54-43f9-a473-507b334907e0");

        /// <summary>
        /// Populates this medication fill instance from the data in the XML.
        /// </summary>
        ///
        /// <param name="typeSpecificXml">
        /// The XML to get the medication fill data from.
        /// </param>
        ///
        /// <exception cref="InvalidOperationException">
        /// If the first node in <paramref name="typeSpecificXml"/> is not
        /// a medication-fill node.
        /// </exception>
        ///
        protected override void ParseXml(IXPathNavigable typeSpecificXml)
        {
            XPathNavigator itemNav =
                typeSpecificXml.CreateNavigator().SelectSingleNode("medication-fill");

            Validator.ThrowInvalidIfNull(itemNav, Resources.MedicationFillUnexpectedNode);

            _name = new CodableValue();
            _name.ParseXml(itemNav.SelectSingleNode("name"));

            _dateFilled =
                XPathHelper.GetOptNavValue<ApproximateDateTime>(itemNav, "date-filled");

            _daysSupply =
                XPathHelper.GetOptNavValueAsInt(itemNav, "days-supply");

            _nextRefillDate =
                XPathHelper.GetOptNavValue<HealthServiceDate>(itemNav, "next-refill-date");

            _refillsLeft =
                XPathHelper.GetOptNavValueAsInt(itemNav, "refills-left");

            _pharmacy =
                XPathHelper.GetOptNavValue<Organization>(itemNav, "pharmacy");

            _prescriptionNumber =
                XPathHelper.GetOptNavValue(itemNav, "prescription-number");

            _lotNumber =
                XPathHelper.GetOptNavValue(itemNav, "lot-number");
        }

        /// <summary>
        /// Writes the medication fill data to the specified XmlWriter.
        /// </summary>
        ///
        /// <param name="writer">
        /// The XmlWriter to write the medication fill data to.
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="writer"/> parameter is <b>null</b>.
        /// </exception>
        ///
        /// <exception cref="ThingSerializationException">
        /// The <see cref="Name"/> property has not been set.
        /// </exception>
        ///
        public override void WriteXml(XmlWriter writer)
        {
            Validator.ThrowIfWriterNull(writer);
            Validator.ThrowSerializationIfNull(_name, Resources.MedicationFillNameNotSet);

            writer.WriteStartElement("medication-fill");

            _name.WriteXml("name", writer);

            XmlWriterHelper.WriteOpt(writer, "date-filled", _dateFilled);
            XmlWriterHelper.WriteOptInt(writer, "days-supply", _daysSupply);
            XmlWriterHelper.WriteOpt(writer, "next-refill-date", _nextRefillDate);
            XmlWriterHelper.WriteOptInt(writer, "refills-left", _refillsLeft);
            XmlWriterHelper.WriteOpt(writer, "pharmacy", _pharmacy);
            XmlWriterHelper.WriteOptString(writer, "prescription-number", _prescriptionNumber);
            XmlWriterHelper.WriteOptString(writer, "lot-number", _lotNumber);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Gets or sets the medication name and clinical code.
        /// </summary>
        ///
        /// <remarks>
        /// The name should be understandable to the person taking the medication, such as the
        /// brand name.
        /// The preferred vocabularies for medication name are "RxNorm" or "NDC".
        /// </remarks>
        ///
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="value"/> is <b>null</b> on set.
        /// </exception>
        ///
        public CodableValue Name
        {
            get { return _name; }

            set
            {
                Validator.ThrowIfArgumentNull(value, nameof(Name), Resources.MedicationFillNameMandatory);
                _name = value;
            }
        }

        private CodableValue _name;

        /// <summary>
        /// Gets or sets the date the prescription was filled.
        /// </summary>
        ///
        /// <remarks>
        /// If the value is not known, it will be set to <b>null</b>.
        /// </remarks>
        ///
        public ApproximateDateTime DateFilled
        {
            get { return _dateFilled; }
            set { _dateFilled = value; }
        }

        private ApproximateDateTime _dateFilled;

        /// <summary>
        /// Gets or sets the number of days supply of the medication.
        /// </summary>
        ///
        /// <remarks>
        /// If the value is not known, it will be set to <b>null</b>.
        /// </remarks>
        ///
        public int? DaysSupply
        {
            get { return _daysSupply; }
            set { _daysSupply = value; }
        }

        private int? _daysSupply;

        /// <summary>
        /// Gets or sets the date on which the prescription can be refilled.
        /// </summary>
        ///
        /// <remarks>
        /// If the value is not known, it will be set to <b>null</b>.
        /// </remarks>
        ///
        public HealthServiceDate NextRefillDate
        {
            get { return _nextRefillDate; }
            set { _nextRefillDate = value; }
        }

        private HealthServiceDate _nextRefillDate;

        /// <summary>
        /// Gets or sets the number of medication refills left.
        /// </summary>
        ///
        /// <remarks>
        /// If the value is not known, it will be set to <b>null</b>.
        /// </remarks>
        ///
        public int? RefillsLeft
        {
            get { return _refillsLeft; }
            set { _refillsLeft = value; }
        }

        private int? _refillsLeft;

        /// <summary>
        /// Gets or sets the pharmacy.
        /// </summary>
        ///
        /// <remarks>
        /// If the value is not known, it will be set to <b>null</b>.
        /// </remarks>
        ///
        public Organization Pharmacy
        {
            get { return _pharmacy; }
            set { _pharmacy = value; }
        }

        private Organization _pharmacy;

        /// <summary>
        /// Gets or sets the free form prescription number.
        /// </summary>
        ///
        /// <remarks>
        /// If the value is not known, it will be set to <b>null</b>.
        /// </remarks>
        ///
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> contains only whitespace.
        /// </exception>
        ///
        public string PrescriptionNumber
        {
            get { return _prescriptionNumber; }

            set
            {
                Validator.ThrowIfStringIsWhitespace(value, "PrescriptionNumber");
                _prescriptionNumber = value;
            }
        }

        private string _prescriptionNumber;

        /// <summary>
        /// Gets or sets the lot number for the medication.
        /// </summary>
        ///
        /// <remarks>
        /// If the value is not known, it will be set to <b>null</b>.
        /// </remarks>
        ///
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> contains only whitespace.
        /// </exception>
        ///
        public string LotNumber
        {
            get { return _lotNumber; }

            set
            {
                Validator.ThrowIfStringIsWhitespace(value, "LotNumber");
                _lotNumber = value;
            }
        }

        private string _lotNumber;

        /// <summary>
        /// Gets a string representation of the medication fill.
        /// </summary>
        ///
        /// <returns>
        /// A string representation of the medication fill.
        /// </returns>
        ///
        public override string ToString()
        {
            string result = Name.ToString();

            if (_dateFilled != null)
            {
                result =
                    string.Format(
                        Resources.MedicationFillToStringFormat,
                        Name.ToString(),
                        _dateFilled.ToString());
            }

            return result;
        }
    }
}

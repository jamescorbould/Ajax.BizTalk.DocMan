namespace Ajax.BizTalk.DocMan.PipelineComponent
{
    using System;
    using System.IO;
    using System.Text;
    using System.Drawing;
    using System.Resources;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;
    using Microsoft.BizTalk.Message.Interop;
    using Microsoft.BizTalk.Component.Interop;
    using Microsoft.BizTalk.Component;
    using Microsoft.BizTalk.Messaging;
    using Microsoft.BizTalk.Streaming;
    using System.Xml.Linq;
    using Microsoft.BizTalk.CAT.BestPractices.Framework.Instrumentation;
    
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [System.Runtime.InteropServices.Guid("31223724-bddb-4143-873e-137027e21638")]
    [ComponentCategory(CategoryTypes.CATID_Encoder)]
    public class NotStreamingBad : Microsoft.BizTalk.Component.Interop.IComponent, IBaseComponent, IPersistPropertyBag, IComponentUI
    {
        
        private System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager("Ajax.BizTalk.DocMan.PipelineComponent.NotStreamingBad", Assembly.GetExecutingAssembly());
        private bool enabled = true;
        
        #region IBaseComponent members
        /// <summary>
        /// Name of the component
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return resourceManager.GetString("COMPONENTNAME", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        /// Version of the component
        /// </summary>
        [Browsable(false)]
        public string Version
        {
            get
            {
                return resourceManager.GetString("COMPONENTVERSION", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        /// Description of the component
        /// </summary>
        [Browsable(false)]
        public string Description
        {
            get
            {
                return resourceManager.GetString("COMPONENTDESCRIPTION", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Switch determining if the component is enabled or not.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
        #endregion
        
        #region IPersistPropertyBag members
        /// <summary>
        /// Gets class ID of component for usage from unmanaged code.
        /// </summary>
        /// <param name="classid">
        /// Class ID of the component
        /// </param>
        public void GetClassID(out System.Guid classid)
        {
            classid = new System.Guid("31223724-bddb-4143-873e-137027e21638");
        }
        
        /// <summary>
        /// not implemented
        /// </summary>
        public void InitNew()
        {
        }
        
        /// <summary>
        /// Loads configuration properties for the component
        /// </summary>
        /// <param name="pb">Configuration property bag</param>
        /// <param name="errlog">Error status</param>
        public virtual void Load(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, int errlog)
        {
            object val = null;

            try
            {
                pb.Read("Enabled", out val, 0);
            }
            catch (System.ArgumentException)
            {
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error reading propertybag: " + ex.Message);
            }

            if (val != null)
            {
                Enabled = (bool)val;
            }
            else
            {
                Enabled = true;
            }
        }
        
        /// <summary>
        /// Saves the current component configuration into the property bag
        /// </summary>
        /// <param name="pb">Configuration property bag</param>
        /// <param name="fClearDirty">not used</param>
        /// <param name="fSaveAllProperties">not used</param>
        public virtual void Save(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, bool fClearDirty, bool fSaveAllProperties)
        {
            object val = (object)Enabled;
            WritePropertyBag(pb, "Enabled", val);
        }
        
        #region utility functionality
        /// <summary>
        /// Reads property value from property bag
        /// </summary>
        /// <param name="pb">Property bag</param>
        /// <param name="propName">Name of property</param>
        /// <returns>Value of the property</returns>
        private object ReadPropertyBag(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, string propName)
        {
            object val = null;
            try
            {
                pb.Read(propName, out val, 0);
            }
            catch (System.ArgumentException )
            {
                return val;
            }
            catch (System.Exception e)
            {
                throw new System.ApplicationException(e.Message);
            }
            return val;
        }
        
        /// <summary>
        /// Writes property values into a property bag.
        /// </summary>
        /// <param name="pb">Property bag.</param>
        /// <param name="propName">Name of property.</param>
        /// <param name="val">Value of property.</param>
        private void WritePropertyBag(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, string propName, object val)
        {
            try
            {
                pb.Write(propName, ref val);
            }
            catch (System.Exception e)
            {
                throw new System.ApplicationException(e.Message);
            }
        }
        #endregion
        #endregion
        
        #region IComponentUI members
        /// <summary>
        /// Component icon to use in BizTalk Editor
        /// </summary>
        [Browsable(false)]
        public IntPtr Icon
        {
            get
            {
                return ((System.Drawing.Bitmap)(this.resourceManager.GetObject("COMPONENTICON", System.Globalization.CultureInfo.InvariantCulture))).GetHicon();
            }
        }
        
        /// <summary>
        /// The Validate method is called by the BizTalk Editor during the build 
        /// of a BizTalk project.
        /// </summary>
        /// <param name="obj">An Object containing the configuration properties.</param>
        /// <returns>The IEnumerator enables the caller to enumerate through a collection of strings containing error messages. These error messages appear as compiler error messages. To report successful property validation, the method should return an empty enumerator.</returns>
        public System.Collections.IEnumerator Validate(object obj)
        {
            // example implementation:
            // ArrayList errorList = new ArrayList();
            // errorList.Add("This is a compiler error");
            // return errorList.GetEnumerator();
            return null;
        }
        #endregion
        
        #region IComponent members
        /// <summary>
        /// Implements IComponent.Execute method.
        /// </summary>
        /// <param name="pc">Pipeline context</param>
        /// <param name="inmsg">Input message</param>
        /// <returns>Original input message</returns>
        /// <remarks>
        /// IComponent.Execute method is used to initiate
        /// the processing of the message in this pipeline component.
        /// </remarks>
        public Microsoft.BizTalk.Message.Interop.IBaseMessage Execute(Microsoft.BizTalk.Component.Interop.IPipelineContext pc, Microsoft.BizTalk.Message.Interop.IBaseMessage inmsg)
        {
            // Note: this is a **NON-OPTIMAL** pipeline component to demonstrate how **NOT** to write a pipeline component!!
            // - It uses XDocument and a string to construct an XML message, which is memory intensive for large messages.
            // - It writes the entire output message into a memory stream in one go, which is memory intensive for large messages.
            // - Further pipeline execution is blocked while this pipeline component completes processing, which slows processing.

            var callToken = TraceManager.PipelineComponent.TraceIn();

            TraceManager.PipelineComponent.TraceInfo(string.Format("{0} - {1} - START NotStreamingBad pipeline component.", System.DateTime.Now, callToken));

            if (Enabled)
            {
                try
                {
                    string xml = string.Empty;
                    byte[] bytesOut = null;

                    TraceManager.PipelineComponent.TraceInfo(string.Format("{0} - {1} - Read message data.", System.DateTime.Now, callToken));

                    BinaryReader binReader = new BinaryReader(inmsg.BodyPart.Data);
                    bytesOut = binReader.ReadBytes((int)inmsg.BodyPart.Data.Length);
                    binReader.Close();

                    xml = System.String.Format(
                            @"<?xml version=""1.0""?>
                            <Invoice xmlns=""http://Invoice/v1"">
                                <Base64EncodedStream>{0}</Base64EncodedStream>
                            </Invoice>"
                            , System.Convert.ToBase64String(bytesOut));

                    // This loads the entire XML into memory, into a DOM.  **NOT OPTIMAL**.
                    TraceManager.PipelineComponent.TraceInfo(string.Format("{0} - {1} - Load XML into memory.", System.DateTime.Now, callToken));

                    XDocument doc = XDocument.Parse(xml);

                    xml = doc.ToString();

                    TraceManager.PipelineComponent.TraceInfo(string.Format("{0} - {1} - Write base64 encoded binary data to stream.", System.DateTime.Now, callToken));

                    Stream ms = new MemoryStream();
                    StreamWriter sw = new StreamWriter(ms, Encoding.ASCII);
                        
                    sw.Write(xml);
                    sw.Flush();

                    inmsg.BodyPart.Data = ms;

                    // Rewind output stream to the beginning, so it's ready to be read.
                    inmsg.BodyPart.Data.Position = 0;

                    pc.ResourceTracker.AddResource(ms);
                }
                catch (Exception ex)
                {
                    TraceManager.PipelineComponent.TraceError(ex, true, callToken);
                    throw;
                }
            }

            TraceManager.PipelineComponent.TraceInfo(string.Format("{0} - {1} - END - NotStreamingBad pipeline component.  Return.", System.DateTime.Now, callToken));

            return inmsg;
        }
        #endregion
    }
}

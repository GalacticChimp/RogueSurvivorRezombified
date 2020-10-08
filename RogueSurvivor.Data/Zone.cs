using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    public class Zone
    {
        Dictionary<string, object> _attributes = null;

        public string Name { get; set; } = "unnamed zone";

        public Rectangle Bounds { get; set; }

        public Zone(string name, Rectangle bounds)
        {
            Name = name ?? throw new ArgumentNullException("name");
            Bounds = bounds;
        }

        public bool HasGameAttribute(string key)
        {
            if (_attributes == null)
            {
                return false; 
            }
            return _attributes.Keys.Contains(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">must be serializable</param>
        public void SetGameAttribute<_T_>(string key, _T_ value)
        {
            if (_attributes == null)
                _attributes = new Dictionary<string, object>(1);

            if (_attributes.Keys.Contains(key))
                _attributes[key] = value;
            else
                _attributes.Add(key, value);
        }

        public _T_ GetGameAttribute<_T_>(string key)
        {
            if (_attributes == null)
                return default(_T_);

            object value;
            if (_attributes.TryGetValue(key, out value))
            {
                if (!(value is _T_))
                    throw new InvalidOperationException("game attribute is not of requested type");
                return (_T_)value;
            }
            else
                return default(_T_);
        }
    }
}

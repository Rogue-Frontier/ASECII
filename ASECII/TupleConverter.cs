using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ASECII {
    //https://stackoverflow.com/a/57319194
    static class STuple {
        static STuple() {
            TypeDescriptor.AddAttributes(typeof((int, int)),  new TypeConverterAttribute(typeof(Int2Converter)));
        }
    }
    /*
    public class TupleConverter<T1, T2> : TypeConverter {
        
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            var elements = Convert.ToString(value).Trim('(').Trim(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return (elements.First(), elements.Last());
        }
    }
    */
    public class Int2Converter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            var elements = Convert.ToString(value).Trim('(').Trim(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return (int.Parse(elements.First()), int.Parse(elements.Last()));
        }
    }
}

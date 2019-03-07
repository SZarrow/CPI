using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPI.ScheduleJobs
{
    public class CPIScheduleConfig
    {
        public static String AppId = "X006";
        public const String AppSecretKey = @"MIIEowIBAAKCAQEA1vPM/xBk8aqN14fdbM0NJSHrEPlVVUYsaD03JsClpOx88bMETYBJ3ygijpk36l5lcnuuUkaSiJ2GlEQ1fkpqJ2RBhhm+mBXAisqA1WVwPOuwsHnX7YE3sxDGLxFfjFVhuxDaRPmnz2/iPBg3lfBKwoA7AKOkuZ7RG78Dds9DFbFg0Hn7GGswxj5TYR3bmat6n9cI0leanR/ql7nmElUqBoAwUl4UxLQBraeC3Cx+2zI3JiLxtAPvz4kaMHlojVyIEFvJQYL8i4tYzmeOc+OSdcjgqOTZGboJBfvSHmFnC9R4tUYZ+nhO8SHrrv2vCHgFH0lbSmc1kSNI23fQcLR9qwIDAQABAoIBACIVlrP7TYZknQlIKfxOp37z2epfHwDel7wPuOcUNS/psAZDdLM1XIFeQ9yIvy0SutNkeUfimOnA0M5B4pmcAykr5Jf3DRngmR9o7PTpmNqQJPxW1b57dvGV/1cHUjdWcqDPE01MqSdjfmQ5Etdbuv8Mhk6bpEsqu83ChDIau3B83Yf1IXguo4Ti/Au9tVOYAbp//Gg5axODCEcXhSwvzPm6gQIH3zXrz60PLrXkB+w/03bijFimdiDuqcUAgO0HS11Sd7SWfgmu9VqL9+dUvarm1MtgR7j8cH5DZ41yQbN3EYdwuQj69aBBzo3wnIw0bVFi3p2h6hDzzkQRbYP6gkECgYEA8ZguWPHHgzZwUStFkt3cglle2jrWVmCPXUEmhZsjTTzwD3uLwyaRIv/K0oBUPLPfZl1YdsdqbGXcdxSS31ikDzJHknpA3uggTqA6ErvMbHoyEuO2DQ2ISZFQQTgF4tc2YgwLizLvvx4OnJy1/Q6lgzjX097OUKEbTovGJmpU258CgYEA48Tw8aNijVoKSl79gUMRQLysCiC49RaTajThgqpPygcQKM64Rj3jqwYt/OVV2U2P6m+QqF6jAMRoYfvrHqY67Qcft7A+s2koc8Z7289tin/plSfKzcpE0+KKtVZyytB8EZLnYWkJSTFKhOEILQdlfNSBS39Kqb6eKv3qYfWfInUCgYBYo5oVnheyR43r6fFr0iSuWnXXoZC0Plc7QsUMbgAEvZ/iPlTn88V6TtkuZFEDuIb1ergTVFTykmjR8+VzNoVy4eKqlloorofz8Qt9hhOZlTe8AHnxzg4716nXU+Os94MHdB3kI3sc5r07rq+CuhX10Cw3mt6dbI6lQdkgjRC3RwKBgDq8/joCnZbAYqj9SDj+l8NvJJrUB37FHK0mCAYPb9Y07hjn/qO2sDDZviBa8EHC+9tEfDS/ex/mhtjGA8N6sPWRgb94RyMzekgpJqXwH1q5U/6wLV/WytstsAHF0oK1M1nA3cTENq3WdVZBRj9+idCgaNuUbyfJTbbeloQ+uJRFAoGBAMPtsgkoNCALCPxaWjkOLwqJtnOFVKVp1I2LUjDdcTifNcepsM+XulmNev4nuysNANDVraYTFZB3/acsCuEiPvVDNOL/RSUWo3J0t5oGqkSzJ3Y/uaORrEYNAivj1jgUE+zuAIAR3Eh6z59l02yTj385HRauFMYPkcO+dPf4mPbY";

        public static String RequestUrl
        {
            get
            {
                String env = ConfigurationManager.AppSettings["Environment"];
                switch (env)
                {
                    case "PrePublish":
                        return "http://cpiprev.hehuadata.com/gateway.c";
                    case "Production":
                        return "https://cpi.hehuadata.com/gateway.c";
                }

                return "http://cpitest.hehuadata.com/gateway.c";
            }
        }
    }
}

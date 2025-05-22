using Queeni.Components.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Interfaces
{
    public interface ICommandParser
    {
        CommandParseResult Parse(string userInput);
    }

}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Innovator.Client.QueryModel.Tests
{
  [TestClass]
  public class TokenizerTests
  {
    private IEnumerable<ODataToken> Tokenize(string value)
    {
      var tokens = new List<ODataToken>();
      using (var tokenizer = new ODataTokenizer(value))
      {
        while (tokenizer.MoveNext())
        {
          tokens.Add(tokenizer.Current);
        }
      }
      return tokens;
    }

    private void VerifySequence(string value, params ODataTokenType[] types)
    {
      var tokens = Tokenize(value).ToArray();
      var actual = tokens.Select(t => t.Type).ToArray();
      CollectionAssert.AreEqual(types, actual);
    }

    [TestMethod]
    public void Tokens_SimpleUrl()
    {
      VerifySequence("http://host/service/Products"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_FunctionNoParam()
    {
      VerifySequence("http://host/service/Products/Model.MostExpensive()"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.OpenParen, ODataTokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_FunctionOneParam()
    {
      VerifySequence("http://host/service/ProductsByCategoryId(categoryId=2)"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Identifier, ODataTokenType.QueryAssign, ODataTokenType.Integer, ODataTokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_FunctionTwoParams()
    {
      VerifySequence("https://host/service/Orders(1)/Items(OrderID=1,ItemNo=2)"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Integer, ODataTokenType.CloseParen
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Identifier, ODataTokenType.QueryAssign, ODataTokenType.Integer, ODataTokenType.Comma
        , ODataTokenType.Identifier, ODataTokenType.QueryAssign, ODataTokenType.Integer, ODataTokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_FunctionWithAlias()
    {
      VerifySequence("http://host/service/ProductsByColor(color=@color)?@color='red'"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Identifier, ODataTokenType.QueryAssign, ODataTokenType.Parameter, ODataTokenType.CloseParen
        , ODataTokenType.Question, ODataTokenType.Parameter, ODataTokenType.QueryAssign, ODataTokenType.String);
    }

    [TestMethod]
    public void Tokens_SingleItem()
    {
      VerifySequence("http://host/service/Categories(1)"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Integer, ODataTokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_SingleItemPath()
    {
      VerifySequence("http://host/service/Products(1)/Supplier"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Integer, ODataTokenType.CloseParen
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_SingleItemFunction()
    {
      VerifySequence("http://host/service/Products(1)/Model.MostRecentOrder()"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Integer, ODataTokenType.CloseParen
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.OpenParen, ODataTokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_UnexpectedQueryString()
    {
      VerifySequence("http://host/service/ProductsByColor(color=@color)?@color='red'&callback=2func&random=3*stuff"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Identifier, ODataTokenType.QueryAssign, ODataTokenType.Parameter
        , ODataTokenType.CloseParen, ODataTokenType.Question, ODataTokenType.Parameter, ODataTokenType.QueryAssign
        , ODataTokenType.String, ODataTokenType.Amperstand, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.Amperstand, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_TwoFunctions()
    {
      VerifySequence("http://host/service/Categories(ID=1)/Products(ID=1)"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Identifier, ODataTokenType.QueryAssign, ODataTokenType.Integer, ODataTokenType.CloseParen
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Identifier, ODataTokenType.QueryAssign, ODataTokenType.Integer, ODataTokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_EscapeString01()
    {
      var url = "http://host/service/People('O''Neil')";
      VerifySequence(url
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.String, ODataTokenType.CloseParen);
      var parts = Tokenize(url).ToArray();
      Assert.AreEqual("O'Neil", parts[8].AsPrimitive());
    }

    [TestMethod]
    public void Tokens_EscapeString02()
    {
      var url = "http://host/service/People(%27O%27%27Neil%27)";
      VerifySequence(url
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.String, ODataTokenType.CloseParen);
      var parts = Tokenize(url).ToArray();
      Assert.AreEqual("O'Neil", parts[8].AsPrimitive());
    }

    [TestMethod]
    public void Tokens_EscapeString03()
    {
      var url = "http://host/service/People%28%27O%27%27Neil%27%29";
      VerifySequence(url
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.String, ODataTokenType.CloseParen);
      var parts = Tokenize(url).ToArray();
      Assert.AreEqual("O'Neil", parts[8].AsPrimitive());
    }

    [TestMethod]
    public void Tokens_EscapeString04()
    {
      var url = "http://host/service/Categories('Smartphone%2FTablet')";
      VerifySequence(url
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.String, ODataTokenType.CloseParen);
      var parts = Tokenize(url).ToArray();
      Assert.AreEqual("Smartphone/Tablet", parts[8].AsPrimitive());
    }

    [TestMethod]
    public void Tokens_Escape()
    {
      var url = "?$callback=jQuery112304312923812233427_1494592722830&%24inlinecount=allpages&%24format=json&%24filter=startswith(tolower(name)%2C%27c+b%27)";
      VerifySequence(url
        , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.Amperstand, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.Amperstand, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.Amperstand, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Identifier, ODataTokenType.OpenParen, ODataTokenType.Identifier, ODataTokenType.CloseParen
        , ODataTokenType.Comma, ODataTokenType.String, ODataTokenType.CloseParen);
      var parts = Tokenize(url).ToArray();
      Assert.AreEqual("c b", parts[parts.Length - 2].AsPrimitive());
    }


    [TestMethod]
    public void Tokens_FilterQuery01()
    {
      VerifySequence("http://host/service/Categories?$filter=Products/$count gt 0"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.Navigation, ODataTokenType.Identifier
        , ODataTokenType.Whitespace, ODataTokenType.GreaterThan, ODataTokenType.Whitespace, ODataTokenType.Integer);
    }

    [TestMethod]
    public void Tokens_FilterQuery02()
    {
      VerifySequence("http://host/service/$all/Model.Customer?$filter=contains(Name,'red')"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.OpenParen, ODataTokenType.Identifier, ODataTokenType.Comma, ODataTokenType.String, ODataTokenType.CloseParen);
    }

    [TestMethod]
    public void Tokens_FilterQuery_BoolLogic()
    {
      VerifySequence("http://host/service/Products?$filter=Name eq 'Milk' and Price lt 2.55"
          , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
          , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
          , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.Equal, ODataTokenType.Whitespace, ODataTokenType.String
          , ODataTokenType.Whitespace, ODataTokenType.And, ODataTokenType.Whitespace, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.LessThan, ODataTokenType.Whitespace, ODataTokenType.Double);
      VerifySequence("http://host/service/Products?$filter=Name eq 'Milk' or Price lt 2.55"
          , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
          , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
          , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.Equal, ODataTokenType.Whitespace, ODataTokenType.String
          , ODataTokenType.Whitespace, ODataTokenType.Or, ODataTokenType.Whitespace, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.LessThan, ODataTokenType.Whitespace, ODataTokenType.Double);
    }

    [TestMethod]
    public void Tokens_FilterQuery_Operators()
    {
      VerifySequence("http://host/service/Products?$filter=Name eq 'Milk'"
          , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
          , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
          , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.Equal, ODataTokenType.Whitespace, ODataTokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name ne 'Milk'"
          , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
          , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
          , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.NotEqual, ODataTokenType.Whitespace, ODataTokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name gt 'Milk'"
          , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
          , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
          , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.GreaterThan, ODataTokenType.Whitespace, ODataTokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name ge 'Milk'"
          , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
          , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
          , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.GreaterThanOrEqual, ODataTokenType.Whitespace, ODataTokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name lt 'Milk'"
          , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
          , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
          , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.LessThan, ODataTokenType.Whitespace, ODataTokenType.String);
      VerifySequence("http://host/service/Products?$filter=Name le 'Milk'"
          , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
          , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
          , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
          , ODataTokenType.Whitespace, ODataTokenType.LessThanOrEqual, ODataTokenType.Whitespace, ODataTokenType.String);
    }


    [TestMethod]
    public void Tokens_FilterQuery_DataTypes()
    {
      var urls = new Dictionary<string, ODataTokenType>()
      {
        { "?$filter=NullValue eq null", ODataTokenType.Null },
        { "?$filter=TrueValue eq true", ODataTokenType.True },
        { "?$filter=FalseValue eq false", ODataTokenType.False },
        { "?$filter=BinaryValue eq binary'T0RhdGE'", ODataTokenType.Base64 },
        { "?$filter=BinaryValue eq X'ffa3cd'", ODataTokenType.Binary },
        { "?$filter=IntegerValue lt -128", ODataTokenType.Integer },
        { "?$filter=IntegerValue lt -128L", ODataTokenType.Long },
        { "?$filter=DoubleValue ge 0.31415926535897931e1", ODataTokenType.Double },
        { "?$filter=DoubleValue ge 0.31415926535897931M", ODataTokenType.Decimal },
        { "?$filter=DoubleValue ge 0.31415926535897931d", ODataTokenType.Double },
        { "?$filter=DoubleValue ge 0.314f", ODataTokenType.Single },
        { "?$filter=SingleValue eq INF", ODataTokenType.PosInfinity },
        { "?$filter=DecimalValue eq 34.95", ODataTokenType.Double },
        { "?$filter=StringValue eq 'Say Hello,then go'", ODataTokenType.String },
        { "?$filter=DateValue eq 2012-12-03", ODataTokenType.Date },
        { "?$filter=DateValue eq datetime'2012-12-03'", ODataTokenType.Date },
        { "?$filter=DateTimeOffsetValue eq 2012-12-03T07:16:23Z", ODataTokenType.Date },
        { "?$filter=DateTimeOffsetValue eq datetimeoffset'2012-12-03T07:16:23Z'", ODataTokenType.Date },
        { "?$filter=DurationValue eq duration'P12DT23H59M59.999999999999S'", ODataTokenType.Duration },
        { "?$filter=DurationValue eq time'P12DT23H59M59.999999999999S'", ODataTokenType.Duration },
        { "?$filter=TimeOfDayValue eq 07:59:59.999", ODataTokenType.TimeOfDay },
        { "?$filter=GuidValue eq 01234567-89ab-cdef-0123-456789abcdef", ODataTokenType.Guid },
        { "?$filter=GuidValue eq guid'01234567-89ab-cdef-0123-456789abcdef'", ODataTokenType.Guid },
        { "?$filter=Int64Value eq 0", ODataTokenType.Integer },
      };

      foreach (var url in urls)
      {
        Assert.AreEqual(url.Value, Tokenize(url.Key).Last().Type);
      }
    }

    [TestMethod]
    public void Tokens_OrderBy01()
    {
      VerifySequence("http://host/service/Categories?$orderby=Products/$count"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.Navigation, ODataTokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_Expand01()
    {
      VerifySequence("http://host/service/Orders?$expand=Customer/Model.VipCustomer"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.Navigation, ODataTokenType.Identifier, ODataTokenType.Period, ODataTokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_Search01()
    {
      VerifySequence("http://host/service/$all?$search=red"
        , ODataTokenType.Scheme, ODataTokenType.PathSeparator, ODataTokenType.Authority
        , ODataTokenType.PathSeparator, ODataTokenType.Identifier, ODataTokenType.PathSeparator, ODataTokenType.Identifier
        , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier);
    }

    [TestMethod]
    public void Tokens_Query()
    {
      VerifySequence("?$format=json&$filter=Name eq 'Apple'"
        , ODataTokenType.Question, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.Amperstand, ODataTokenType.QueryName, ODataTokenType.QueryAssign, ODataTokenType.Identifier
        , ODataTokenType.Whitespace, ODataTokenType.Equal, ODataTokenType.Whitespace, ODataTokenType.String);
    }
  }
}

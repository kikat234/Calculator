using System.Globalization;
using System.Runtime.Intrinsics.X86;
namespace Calculator.Pages;

public partial class CalculatorPage : ContentPage
{
    string _current = "0";
	double? _left;
	string? _op;
	bool _startNew;
	bool _error;
	public CalculatorPage()
	{
		InitializeComponent();
	}
	void Refresh(string expression = "")
	{
		ResultLabel.Text = _current;
		ExpressionLabel.Text = expression;
    }
    static string Fmt(double value)
    {
        return value.ToString("G12", CultureInfo.InvariantCulture);
    }
	double Value() => double.Parse(_current, CultureInfo.InvariantCulture);
	string Expr() => _left is null ? "" : $"{Fmt(_left.Value)} {_op}";

	// Apres erreur, tout repart a zero
	bool RestIfError()
	{
		if (!_error) return false;
		OnClear(this, EventArgs.Empty);
        return true;
    }
	//chiffre ET decimal
	void OnDigit(object? sender, EventArgs e)
    {
        RestIfError();
        var d = ((Button)sender!).Text;
        if (_startNew || _current == "0")
        {
            _current = d;
            _startNew = false;
        }
        
        else if (_current.Length < 15) _current += d;

        Refresh(Expr());
    }

	void OnDecimal(object? sender, EventArgs e)
	{
		RestIfError();
		if (_startNew) {
            _current = "0.";
            _startNew = false;
        }
        else if (!_current.Contains('.')) _current += '.';
		Refresh(Expr());
    }

	// Operateurs
	void OnOperator(object? sender, EventArgs e)
	{
		if (RestIfError()) return;
		var op = ((Button)sender!).Text;

		//enchainement 2+ 3 + ... on calcule le resultat de 2+3
		if (_left is null && _op is not null && !_startNew)
		{
			if (!TryCompute(_left.Value, Value(), _op, out var r)) return;
			_left = r;
		}
		else _left = Value();
		_op = op;
		_current = Fmt(_left.Value);
        _startNew = true;
        Refresh(Expr());
	}

	void OnEquals(object? sender, EventArgs e)
	{
		if (RestIfError() || _left is null || _op is null) return;
		double right = Value();
		string expr = $"{Fmt(_left.Value)} {_op} {Fmt(right)} =";
		if (!TryCompute(_left.Value, right, _op, out var r)) return;
		_current = Fmt(r);
		_left = null;
		_op = null;
		_startNew = true;
		Refresh(expr);
	}

    //Retourne false en cas d'erreur (division par 0)
	bool TryCompute(double left, double right, string op, out double result)
	{
		result = 0;
		switch (op)
        {
            case "+": result = left + right; break;
            case "-": result = left - right; break;
            case "×": result = left * right; break;
            case "÷":
                if (right == 0)
                {
                    _error = true;
					_left = null;
					_op = null;
                    _startNew = true;
                    _current = "Erreur, Division par zéro impossible";
                    
                    Refresh();
                    return false;
                }
                result = left / right; break;
            
        }
        return true;
    }

	//Fonctions

    void OnClear(object? sender, EventArgs e)
	{
		_current = "0";
		_left = null;
		_op = null;
		_startNew = false;
		_error = false;
		Refresh();
	}
	void OnBackspace(object? sender, EventArgs e)
    {
        if (RestIfError() || _startNew) return;
        _current = _current.Length <= 1 || (_current.Length == 2 && _current[0] == '0') ? "0" : _current[..^1];
        Refresh(Expr());
    }
	void OnSign(object? sender, EventArgs e)
    {
        if (RestIfError() || _current == "0") return;
        _current = _current.StartsWith('-') ? _current[1..] : "-" + _current;
        Refresh(Expr());
    }
	void OnPercent(object? sender, EventArgs e)
    {
        if (RestIfError()) return;
        double value = Value() / 100;
        _current = Fmt(value);
        _startNew = true;
        Refresh(Expr());
    }
    
}
# Accounts Normaliser

Command-line tool for normalising downloaded transactions from CSV and QIF into OFX.

## Synopsis

```
dotnet run [-c|--config]
```

## Options

* `-c|--config <PATH>`

  Specifies the location of the configuration file (default value: `config.json`)

## Configuration

* `Accounts` (object)
  * `<NAME>` (object) name is unimportant but can be used to identify different accounts or input formats to be processed
    * `Source` (string) full path to read input files
    * `SourcePrefix` (string, optional) file name prefix to limit processing to only some files in `Source`
    * `SourceSuffix` (string, optional) file name suffix to limit processing to only some files in `Source`
    * `SourceFormat` (object)
      * `Type` (string) file extension for input files (possible values: `csv`, `qif`)
      * `BankID` (string, template, optional) bank identification (e.g. sort code)
      * `AccountID` (string, template, optional) account identification
      * `AccountType` (string, template, optional) account type (possible values: `Unknown`, `Checking`, `Savings`, `MoneyMarket`, `CreditLine`)
      * `Currency` (string, template, optional) currency name (e.g. `GBP`)
      * `Deposit` (string, template, optional) amount added to account
      * `Withdrawal` (string, template, optional) amount removed from account
      * `Amount` (string, template, optional) amount added to or removed from account (must also set `AmountType`, `AmountTypeDeposit`, `AmountTypeWithdrawal`)
      * `AmountType` (string, template, optional) column containing value indicating add/remove
      * `AmountTypeDeposit` (string, optional) value used in `AmountType` column to indicate `Amount` is added to account
      * `AmountTypeWithdrawal` (string, optional) value used in `AmountType` column to indicate `Amount` is removed from account
      * `AmountTypeDefault` (string, optional) value used for `AmountType` if column is missing (only useful if mixed formats)
      * `Date` (string, template, optional) date of transaction (dates are parsed by [.NET DateTimeOffset.Parse](https://docs.microsoft.com/en-us/dotnet/api/system.datetimeoffset.parse))
      * `Description` (string, template, optional) description for transaction
      * `Memo` (string, template, optional)
    * `Target` (string) full path to write output files
    * `TargetPrefix` (string, optional) file name prefix to add to output files
    * `TargetSuffix` (string, optional) file name suffix to add to output files
    * `TargetFormat` (object)
      * `Type` (string) file extension for output files (possible values: `ofx`)

### Format-specific notes

* `CSV` supports both deposit and withdrawal columns in the same file, but one must be zero in each row
* `QIF` only supports checking accounts

### SourceFormat templates

All the options for `SourceFormat` (except `Type`) support the following templates:

* `$column$` + column name: extract value from the named column (e.g. `$column$currency` means extract value from `currency` column)
* Anything else: use value in configuration directly

## Example configuration

```json
{
    "Accounts": {
        "Credit Card CSV": {
            "Source": "C:\\Accounting\\Credit Card",
            "SourcePrefix": "transactions-",
            "SourceFormat": {
                "Type": "csv",
                "BankID": "12-34-56",
                "AccountID": "01234567",
                "AccountType": "Checking",
                "Currency": "$column$currency",
                "Date": "$column$date",
                "Description": "$column$description",
                "Amount": "$column$amount",
                "AmountType": "$column$debitCreditCode",
                "AmountTypeDeposit": "Credit",
                "AmountTypeWithdrawal": "Debit"
            },
            "Target": "C:\\Accounting\\Normalised",
            "TargetFormat": {
                "Type": "ofx"
            }
        }
    }
}
```

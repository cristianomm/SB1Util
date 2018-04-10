using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SB1Util.UI
{
    public enum SAPMenuItems
    {
 //       Administration
 // 3328
 
 //Choose Company
 // 3329
 
 //Exchange Rates and Indexes
 // 3333
 
 //System Initialization
 // 8192
 
 //Company Details
 // 8193
 
 //General Settings
 // 8194
 
 //Posting Periods
 // 1596
 
 //Authorizations
 // 43521
 
 //General Authorizations
 // 3332
 
 //Additional Authorization Creator
 // 3342
 
 //Data Ownership Authorizations
 // 3340
 
 //Data Ownership Exceptions
 // 3341
 
 //Document Numbering
 // 8195
 
 //Document Settings
 // 8196
 
 //Print Preferences
 // 8197
 
 //Opening Balances
 // 43522
 
 //G/L Accounts Opening Balance
 // 8200
 
 //Business Partners Opening Balance
 // 2564
 
 //1099 Opening Balance
 // 10764
 
 //Setup
 // 43525
 
 //General
 // 8448
 
 //Password Administration
 // 1588
 
 //Users
 // 8449
 
 //Change Password
 // 4128
 
 //Sales Employees/Buyers
 // 8454
 
 //Territories
 // 8713
 
 //Commission Groups
 // 8453
 
 //Predefined Text
 // 43571
 
 //Languages
 // 1798
 
 //Freight
 // 1560
 
 //Financials
 // 43526
 
 //Edit Chart of Accounts
 // 4116
 
 //G/L Account Determination
 // 8199
 
 //Account Segmentation
 // 8461
 
 //Currencies
 // 8450
 
 //Transaction Codes
 // 8455
 
 //Projects
 // 8457
 
 //Period Indicators
 // 8210
 
 //1099 Table
 // 10763
 
 //Doubtful Debts
 // 8464
 
 //Tax
 // 15616
 
 //Sales Tax Jurisdiction Types
 // 15617
 
 //Sales Tax Jurisdictions
 // 15618
 
 //Sales Tax Codes
 // 15619
 
 //Sales Opportunities
 // 17152
 
 //Sales Stages
 // 17153
 
 //Partners
 // 17154
 
 //Competitors
 // 17155
 
 //Relationships
 // 17156
 
 //Purchasing
 // 43527
 
 //Landed Costs
 // 8456
 
 //Business Partners
 // 43528
 
 //Countries
 // 8459
 
 //Address Formats
 // 8460
 
 //Customer Groups
 // 10753
 
 //Vendor Groups
 // 10754
 
 //Business Partner Properties
 // 10755
 
 //Business Partner Priorities
 // 10765
 
 //Dunning Levels
 // 10766
 
 //Dunning Terms
 // 10769
 
 //Payment Terms
 // 8452
 
 //Payment Blocks
 // 10767
 
 //Banking
 // 11264
 
 //Banks
 // 11265
 
 //House Bank Accounts
 // 1553
 
 //Credit Cards
 // 11266
 
 //Credit Card Payment
 // 11267
 
 //Credit Card Payment Methods
 // 11268
 
 //Bank Charges Allocation Codes
 // 1561
 
 //Payment Methods
 // 16897
 
 //Payment Run Defaults
 // 16898
 
 //Inventory
 // 11520
 
 //Item Groups
 // 11521
 
 //Item Properties
 // 11522
 
 //Warehouses
 // 11523
 
 //Length and Width UoM
 // 11524
 
 //Weight UoM
 // 11525
 
 //Customs Groups
 // 11526
 
 //Manufacturers
 // 11527
 
 //Shipping Types
 // 11528
 
 //Locations
 // 11529
 
 //Inventory Cycles
 // 11530
 
 //Package Types
 // 11532
 
 //Service
 // 43529
 
 //Contract Templates
 // 3601
 
 //Queues
 // 8712
 
 //Data Import/Export
 // 43530
 
 //Data Import
 // 8960
 
 //Import from Excel
 // 8961
 
 //Import Transactions from SAP Business One
 // 8962
 
 //Data Export
 // 9216
 
 //Export Transactions to SAP Business One
 // 9217
 
 //Utilities
 // 8704
 
 //Period-End Closing
 // 8705
 
 //Update Control Report
 // 8709
 
 //Check Document Numbering
 // 13062
 
 //Approval Procedures
 // 14848
 
 //Approval Stages
 // 14849
 
 //Approval Templates
 // 14850
 
 //Approval Status Report
 // 14851
 
 //Approval Decision Report
 // 14852
 
 //License
 // 43524
 
 //License Administration
 // 8208
 
 //Add-On Identifier Generator
 // 8209
 
 //Add-Ons
 // 43523
 
 //Add-On Manager
 // 8201
 
 //Add-On Administration
 // 8202
 
 //Alerts Management
 // 3338
 
 //Financials
 // 1536
 
 //Chart of Accounts
 // 1537
 
 //Edit Chart of Accounts
 // 1538
 
 //Account Code Generator
 // 1539
 
 //Journal Entry
 // 1540
 
 //Journal Vouchers
 // 1541
 
 //Posting Templates
 // 1542
 
 //Recurring Postings
 // 1543
 
 //Reverse Transactions
 // 1552
 
 //Exchange Rate Differences
 // 1545
 
 //Conversion Differences
 // 1546
 
 //1099 Editing
 // 2572
 
 //Financial Report Templates
 // 1551
 
 //Internal Reconciliations
 // 9460
 
 //Reconciliation
 // 9461
 
 //Manage Previous Reconciliations
 // 1590
 
 //Budget Setup
 // 10496
 
 //Budget Scenarios
 // 10497
 
 //Budget Distribution Methods
 // 10498
 
 //Budget
 // 10499
 
 //Cost Accounting
 // 1792
 
 //Profit Centers
 // 1793
 
 //Distribution Rules
 // 1794
 
 //Table of Profit Centers and Distribution Rules
 // 1795
 
 //Profit Center Report
 // 1796
 
 //Financial Reports
 // 43531
 
 //Accounting
 // 13056
 
 //G/L Accounts and Business Partners
 // 13057
 
 //General Ledger
 // 13058
 
 //Aging
 // 4096
 
 //Customer Receivables Aging
 // 4098
 
 //Vendor Liabilities Aging
 // 4099
 
 //Transaction Journal Report
 // 1544
 
 //Transaction Report by Projects
 // 13064
 
 //Locate Journal Transaction by Amount Range
 // 45067
 
 //Locate Journal Transaction by FC Amount Range
 // 45082
 
 //Transactions Received from Voucher Report
 // 45087
 
 //Document Journal
 // 13065
 
 //1099/1096 Report
 // 13070
 
 //Tax
 // 43532
 
 //Tax Report
 // 13072
 
 //Financial
 // 9728
 
 //Balance Sheet
 // 9729
 
 //Trial Balance
 // 9730
 
 //Profit and Loss Statement
 // 9731
 
 //Cash Flow
 // 4101
 
 //Comparison
 // 1648
 
 //Balance Sheet Comparison
 // 1649
 
 //Trial Balance Comparison
 // 1650
 
 //Profit and Loss Statement Comparison
 // 1651
 
 //Budget Reports
 // 10240
 
 //Budget Report
 // 4608
 
 //Balance Sheet Budget Report
 // 10241
 
 //Trial Balance Budget Report
 // 10242
 
 //Profit and Loss Statement Budget Report
 // 10243
 
 //Sales Opportunities
 // 2560
 
 //Sales Opportunity
 // 2566
 
 //Sales Opportunities Reports
 // 43533
 
 //Opportunities Forecast Report
 // 2578
 
 //Opportunities Forecast Over Time Report
 // 2580
 
 //Opportunities Statistics Report
 // 2579
 
 //Opportunities Report
 // 2577
 
 //Stage Analysis
 // 2568
 
 //Information Source Distribution Over Time Report
 // 2574
 
 //Won Opportunities Report
 // 2569
 
 //Lost Opportunities Report
 // 2573
 
 //My Open Opportunities Report
 // 2575
 
 //My Closed Opportunities Report
 // 2576
 
 //Opportunities Pipeline
 // 2570
 
 //Sales - A/R
 // 2048
 
 //Sales Quotation
 // 2049
 
 //Sales Order
 // 2050
 
 //Delivery
 // 2051
 
 //Return
 // 2052
 
 //A/R Down Payment Invoice
 // 2071
 
 //A/R Invoice
 // 2053
 
 //A/R Invoice + Payment
 // 2054
 
 //A/R Credit Memo
 // 2055
 
 //A/R Reserve Invoice
 // 2056
 
 //Document Generation Wizard
 // 2059
 
 //Document Printing
 // 2058
 
 //Dunning Wizard
 // 2063
 
 //Sales Reports
 // 12800
 
 //Open Items List
 // 4097
 
 //Blank
 // 45078
 
 //Document Drafts Report
 // 2061
 
 //Sales Analysis
 // 12801
 
 //Backorder
 // 1562
 
 //Locate Exceptional Discount in Invoice
 // 45064
 
 //SP Commission by Invoices in Posting Date Cross-Section
 // 45084
 
 //Sales Order Without Deposit
 // 45091
 
 //Sales Order Linked to Deposit
 // 45093
 
 //Purchasing - A/P
 // 2304
 
 //Purchase Order
 // 2305
 
 //Goods Receipt PO
 // 2306
 
 //Goods Return
 // 2307
 
 //A/P Down Payment Invoice
 // 2317
 
 //A/P Invoice
 // 2308
 
 //A/P Credit Memo
 // 2309
 
 //A/P Reserve Invoice
 // 2314
 
 //Landed Costs
 // 2310
 
 //Document Printing
 // 2312
 
 //Purchasing Reports
 // 43534
 
 //Open Items List
 // 1547
 
 //Blank
 // 45076
 
 //Document Drafts Report
 // 2313
 
 //Purchase Analysis
 // 12802
 
 //Locate Exceptional Discount in Invoice
 // 45065
 
 //SP Commission by Invoices in Posting Date Cross-Section
 // 45085
 
 //Purchase Order Without Deposit
 // 45095
 
 //Purchase Order Linked to Deposit
 // 45097
 
 //Business Partners
 // 43535
 
 //Business Partner Master Data
 // 2561
 
 //Activity
 // 2563
 
 //Internal Reconciliations
 // 9458
 
 //Reconciliation
 // 9459
 
 //Manage Previous Reconciliations
 // 1591
 
 //Business Partner Reports
 // 43536
 
 //My Activities
 // 10771
 
 //Activities Overview
 // 2565
 
 //Inactive Customers
 // 14338
 
 //Dunning History Report
 // 2068
 
 //Customer Receivables by Customer Cross-Section
 // 45060
 
 //Customers Credit Limit Deviation
 // 45089
 
 //Aging
 // 43548
 
 //Customer Receivables Aging
 // 4112
 
 //Vendor Liabilities Aging
 // 4113
 
 //Internal Reconciliation
 // 51199
 
 //Internal Reconciliation by Due Date
 // 45071
 
 //Internal Reconciliation by Exact Amount
 // 45099
 
 //Internal Reconciliation by Trans. Number
 // 45101
 
 //Banking
 // 43537
 
 //Incoming Payments
 // 2816
 
 //Incoming Payments
 // 2817
 
 //Check Register
 // 2823
 
 //Credit Card Management
 // 2824
 
 //Deposits
 // 14592
 
 //Deposit
 // 14593
 
 //Outgoing Payments
 // 43538
 
 //Outgoing Payments
 // 2818
 
 //Checks for Payment
 // 2820
 
 //Void Checks for Payment
 // 2822
 
 //Checks for Payment Drafts
 // 2821
 
 //Payment Wizard
 // 16899
 
 //Bank Statements and External Reconciliations
 // 11008
 
 //Process External Bank Statement
 // 11009
 
 //Reconciliation
 // 9457
 
 //Manual Reconciliation
 // 1797
 
 //Manage Previous External Reconciliations
 // 11011
 
 //Check and Restore Previous External Reconciliations
 // 11012
 
 //Check Number Confirmation
 // 1554
 
 //Document Printing
 // 2829
 
 //Banking Reports
 // 51197
 
 //Check Register Report
 // 2834
 
 //Payment Drafts Report
 // 2831
 
 //Checks for Payment in Date Cross Section Report
 // 45073
 
 //External Reconciliation
 // 51195
 
 //Locate Reconciliation in Bank Statement by Row Number
 // 45072
 
 //Locate Reconciliation/Row in Bank Statements by Exact Amount
 // 45080
 
 //External Reconciliation by Due Date
 // 45103
 
 //External Reconciliation by Exact Sum
 // 45105
 
 //External Reconciliation by Sum (FC)
 // 45107
 
 //External Reconciliation by Trans. Number
 // 45109
 
 //Inventory
 // 3072
 
 //Item Master Data
 // 3073
 
 //Item Management
 // 15872
 
 //Serial Numbers
 // 12032
 
 //Serial Number Management
 // 12033
 
 //Serial Number Details
 // 12034
 
 //Batches
 // 12288
 
 //Batch Management
 // 12289
 
 //Batch Details
 // 12290
 
 //Alternative Items
 // 11531
 
 //Business Partner Catalog Numbers
 // 12545
 
 //Global Update to BP Catalog Numbers
 // 12546
 
 //Inventory Valuation Method
 // 12547
 
 //Inventory Transactions
 // 43540
 
 //Goods Receipt
 // 3078
 
 //Goods Issue
 // 3079
 
 //Inventory Transfer
 // 3080
 
 //Initial Quantities, Inventory Tracking, and Stock Posting
 // 3081
 
 //Cycle Count Recommendations
 // 3085
 
 //Inventory Revaluation
 // 3086
 
 //Price Lists
 // 43541
 
 //Price Lists
 // 3076
 
 //Period and Volume Discounts
 // 11781
 
 //Special Prices
 // 11776
 
 //Special Prices for Business Partners
 // 11777
 
 //Copy Special Prices to Selection Criteria
 // 11778
 
 //Update Special Prices Globally
 // 11779
 
 //Discount Groups
 // 11780
 
 //Update Parent Item Prices Globally
 // 11782
 
 //Pick and Pack
 // 16640
 
 //Pick and Pack Manager
 // 16641
 
 //Pick List
 // 16642
 
 //Inventory Reports
 // 1760
 
 //Items List
 // 1761
 
 //Document Drafts Report
 // 2313
 
 //Last Prices Report
 // 1713
 
 //Inactive Items
 // 1715
 
 //Inventory Posting List
 // 1762
 
 //Inventory Status
 // 1763
 
 //Inventory in Warehouse Report
 // 1764
 
 //Inventory Audit Report
 // 1549
 
 //Inventory Valuation Report
 // 1765
 
 //Serial Number Transactions Report
 // 1779
 
 //Batch Number Transactions Report
 // 1747
 
 //Production
 // 4352
 
 //Bill of Materials
 // 4353
 
 //Production Order
 // 4369
 
 //Receipt from Production
 // 4370
 
 //Issue for Production
 // 4371
 
 //Update Parent Item Prices Globally
 // 4358
 
 //Production Reports
 // 43542
 
 //Bill of Materials Report
 // 4357
 
 //MRP
 // 43543
 
 //Forecasts
 // 4360
 
 //MRP Wizard
 // 4361
 
 //Order Recommendation
 // 4368
 
 //Service
 // 3584
 
 //Service Call
 // 3587
 
 //Customer Equipment Card
 // 3591
 
 //Service Contract
 // 3585
 
 //Solutions Knowledge Base
 // 3589
 
 //Service Reports
 // 7680
 
 //Service Calls
 // 7684
 
 //Service Calls by Queue
 // 7698
 
 //Response Time by Assigned to
 // 7699
 
 //Average Closure Time
 // 7693
 
 //Service Contracts
 // 7682
 
 //Customer Equipment Card Report
 // 3596
 
 //Service Monitor
 // 7691
 
 //My Service Calls
 // 7689
 
 //My Open Service Calls
 // 7688
 
 //My Overdue Service Calls
 // 7690
 
 //Human Resources
 // 43544
 
 //Employee Master Data
 // 3590
 
 //Human Resources Reports
 // 16128
 
 //Employee List
 // 7694
 
 //Absence Report
 // 7696
 
 //Phone Book
 // 7695
 
 //Reports
 // 43545
 
 //Financials
 // 43546
 
 //Accounting
 // 43547
 
 //G/L Accounts and Business Partners
 // 1617
 
 //General Ledger
 // 1618
 
 //Aging
 // 43548
 
 //Customer Receivables Aging
 // 4112
 
 //Vendor Liabilities Aging
 // 4113
 
 //Transaction Journal Report
 // 4114
 
 //Transaction Report by Projects
 // 1624
 
 //Locate Journal Transaction by Amount Range
 // 45066
 
 //Locate Journal Transaction by FC Amount Range
 // 45081
 
 //Transactions Received from Voucher Report
 // 45086
 
 //Document Journal
 // 1625
 
 //1099/1096 Report
 // 1630
 
 //Tax
 // 43549
 
 //Tax Report
 // 1632
 
 //Financial
 // 43550
 
 //Balance Sheet
 // 13313
 
 //Trial Balance
 // 13314
 
 //Profit and Loss Statement
 // 13315
 
 //Cash Flow
 // 4115
 
 //Comparison
 // 43551
 
 //Balance Sheet Comparison
 // 9985
 
 //Trial Balance Comparison
 // 9986
 
 //Profit and Loss Statement Comparison
 // 9987
 
 //Budget Reports
 // 10240
 
 //Budget Report
 // 4624
 
 //Balance Sheet Budget Report
 // 1681
 
 //Trial Balance Budget Report
 // 1682
 
 //Profit and Loss Statement Budget Report
 // 1683
 
 //Sales Opportunities
 // 43553
 
 //Opportunities Forecast Report
 // 2684
 
 //Opportunities Forecast Over Time Report
 // 2692
 
 //Opportunities Statistics Report
 // 2689
 
 //Opportunities Report
 // 2683
 
 //Stage Analysis
 // 2680
 
 //Information Source Distribution Over Time Report
 // 2686
 
 //Won Opportunities Report
 // 2681
 
 //Lost Opportunities Report
 // 2685
 
 //My Open Opportunities Report
 // 2690
 
 //My Closed Opportunities Report
 // 2691
 
 //Opportunities Pipeline
 // 2682
 
 //Sales and Purchasing
 // 43554
 
 //Open Items List
 // 1548
 
 //Blank
 // 45077
 
 //Document Drafts Report
 // 2313
 
 //Backorder
 // 11616
 
 //Sales Analysis
 // 1697
 
 //Purchase Analysis
 // 1698
 
 //Locate Exceptional Discount in Invoice
 // 45063
 
 //SP Commission by Invoices in Posting Date Cross-Section
 // 45083
 
 //Sales Order Without Deposit
 // 45090
 
 //Sales Order Linked to Deposit
 // 45092
 
 //Purchase Order Without Deposit
 // 45094
 
 //Purchase Order Linked to Deposit
 // 45096
 
 //Business Partners
 // 43555
 
 //My Activities
 // 10772
 
 //Activities Overview
 // 4118
 
 //Inactive Customers
 // 1714
 
 //Dunning History Report
 // 2069
 
 //Customer Receivables by Customer Cross-Section
 // 45059
 
 //Customers Credit Limit Deviation
 // 45088
 
 //Aging
 // 43548
 
 //Customer Receivables Aging
 // 4112
 
 //Vendor Liabilities Aging
 // 4113
 
 //Internal Reconciliation
 // 51188
 
 //Internal Reconciliation by Due Date
 // 45061
 
 //Internal Reconciliation by Exact Amount
 // 45098
 
 //Internal Reconciliation by Trans. Number
 // 45100
 
 //Banking
 // 51196
 
 //Check Register Report
 // 2834
 
 //Payment Drafts Report
 // 2832
 
 //Checks for Payment in Date Cross Section Report
 // 45058
 
 //External Reconciliation
 // 51191
 
 //Locate Reconciliation in Bank Statement by Row Number
 // 45062
 
 //Locate Reconciliation/Row in Bank Statements by Exact Amount
 // 45079
 
 //External Reconciliation by Due Date
 // 45102
 
 //External Reconciliation by Exact Sum
 // 45104
 
 //External Reconciliation by Sum (FC)
 // 45106
 
 //External Reconciliation by Trans. Number
 // 45108
 
 //Inventory
 // 14080
 
 //Items List
 // 14081
 
 //Last Prices Report
 // 14337
 
 //Inactive Items
 // 14339
 
 //Inventory Posting List
 // 14082
 
 //Inventory Status
 // 14083
 
 //Inventory in Warehouse Report
 // 14084
 
 //Inventory Audit Report
 // 1550
 
 //Inventory Valuation Report
 // 14085
 
 //Serial Number Transactions Report
 // 12035
 
 //Batch Number Transactions Report
 // 12291
 
 //Production
 // 43557
 
 //Bill of Materials Report
 // 4121
 
 //Service
 // 43556
 
 //Service Calls
 // 3588
 
 //Service Calls by Queue
 // 3602
 
 //Response Time by Assigned to
 // 3603
 
 //Average Closure Time
 // 3597
 
 //Service Contracts
 // 3586
 
 //Customer Equipment Card Report
 // 7692
 
 //Service Monitor
 // 3595
 
 //My Service Calls
 // 3593
 
 //My Open Service Calls
 // 3592
 
 //My Overdue Service Calls
 // 3594
 
 //Human Resources
 // 43558
 
 //Employee List
 // 3598
 
 //Absence Report
 // 3600
 
 //Phone Book
 // 3599
 
 //Legal Lists
 // 43559
 
 //Legal Lists
 // 3604
 

    }
}

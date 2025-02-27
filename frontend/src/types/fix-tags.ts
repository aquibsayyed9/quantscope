// Create a separate file: types/fix-tags.ts

export const FIX_TAGS: Record<string, string> = {
    '1': 'Account',
    '2': 'AdvId',
    '3': 'AdvRefID',
    '4': 'AdvSide',
    '5': 'AdvTransType',
    '6': 'AvgPx',
    '7': 'BeginSeqNo',
    '8': 'BeginString',
    '9': 'BodyLength',
    '10': 'CheckSum',
    '11': 'ClOrdID',
    '12': 'Commission',
    '13': 'CommType',
    '14': 'CumQty',
    '15': 'Currency',
    '16': 'EndSeqNo',
    '17': 'ExecID',
    '18': 'ExecInst',
    '19': 'ExecRefID',
    '20': 'HandlInst',
    '21': 'SecurityIDSource',
    '22': 'SecurityIDSource',
    '23': 'IOIid',
    '24': 'IOIQltyInd',
    '25': 'IOIRefID',
    '26': 'IOIQty',
    '27': 'IOITransType',
    '28': 'LastCapacity',
    '29': 'LastMkt',
    '30': 'LastPx',
    '31': 'LastQty',
    '32': 'LineNo',
    '33': 'ListSeqNo',
    '34': 'MsgSeqNum',
    '35': 'MsgType',
    '36': 'NewSeqNo',
    '37': 'OrderID',
    '38': 'OrderQty',
    '39': 'OrdStatus',
    '40': 'OrdType',
    '41': 'OrigClOrdID',
    '42': 'OrigTime',
    '43': 'PossDupFlag',
    '44': 'Price',
    '45': 'RefSeqNum',
    '46': 'RelatdSym',
    '47': 'Rule80A',
    '48': 'SecurityID',
    '49': 'SenderCompID',
    '50': 'SenderSubID',
    '51': 'SendingDate',
    '52': 'SendingTime',
    '53': 'Quantity',
    '54': 'Side',
    '55': 'Symbol',
    '56': 'TargetCompID',
    '57': 'TargetSubID',
    '58': 'Text',
    '59': 'TimeInForce',
    '60': 'TransactTime',
    '61': 'Urgency',
    '62': 'ValidUntilTime',
    '63': 'SettlType',
    '64': 'SettlDate',
    '65': 'SymbolSfx',
    '66': 'ListID',
    '67': 'ListSeqNo',
    '68': 'TotNoOrders',
    '69': 'ListExecInst',
    '70': 'AllocID',
    '71': 'AllocTransType',
    '72': 'RefAllocID',
    '73': 'NoOrders',
    '74': 'AvgPxPrecision',
    '75': 'TradeDate',
    '76': 'ExecBroker',
    '77': 'OpenClose',
    '78': 'NoAllocs',
    '79': 'AllocAccount',
    '80': 'AllocQty',
    '81': 'ProcessCode',
    '82': 'NoRpts',
    '83': 'RptSeq',
    '84': 'CxlQty',
    '85': 'NoDlvyInst',
    '86': 'DlvyInst',
    '87': 'AllocStatus',
    '88': 'AllocRejCode',
    '89': 'Signature',
    '90': 'SecureDataLen',
    '91': 'SecureData',
    '92': 'BrokerOfCredit',
    '93': 'SignatureLength',
    '94': 'EmailType',
    '95': 'RawDataLength',
    '96': 'RawData',
    '97': 'PossResend',
    '98': 'EncryptMethod',
    '99': 'StopPx',
    '100': 'ExDestination',
    '102': 'CxlRejReason',
    '103': 'OrdRejReason',
    '104': 'IOIQualifier',
    '105': 'WaveNo',
    '106': 'Issuer',
    '107': 'SecurityDesc',
    '108': 'HeartBtInt',
    '109': 'ClientID',
    '110': 'MinQty',
    '111': 'MaxFloor',
    '112': 'TestReqID',
    '113': 'ReportToExch',
    '114': 'LocateReqd',
    '115': 'OnBehalfOfCompID',
    '116': 'OnBehalfOfSubID',
    '117': 'QuoteID',
    '118': 'NetMoney',
    '119': 'SettlCurrAmt',
    '120': 'SettlCurrency',
    '121': 'ForexReq',
    '122': 'OrigSendingTime',
    '123': 'GapFillFlag',
    '124': 'NoExecs',
    '125': 'CxlType',
    '126': 'ExpireTime',
    '127': 'DKReason',
    '128': 'DeliverToCompID',
    '129': 'DeliverToSubID',
    '130': 'IOINaturalFlag',
    '131': 'QuoteReqID',
    '132': 'BidPx',
    '133': 'OfferPx',
    '134': 'BidSize',
    '135': 'OfferSize',
    '136': 'NoMiscFees',
    '137': 'MiscFeeAmt',
    '138': 'MiscFeeCurr',
    '139': 'MiscFeeType',
    '140': 'PrevClosePx',
    '141': 'ResetSeqNumFlag',
    '142': 'SenderLocationID',
    '143': 'TargetLocationID',
    '144': 'OnBehalfOfLocationID',
    '145': 'DeliverToLocationID',
    '146': 'NoRelatedSym',
    '147': 'Subject',
    '148': 'Headline',
    '149': 'URLLink',
    '150': 'ExecType',
    '151': 'LeavesQty',
    '152': 'CashOrderQty',
    '153': 'AllocAvgPx',
    '154': 'AllocNetMoney',
    '155': 'SettlCurrFxRate',
    '156': 'SettlCurrFxRateCalc',
    '157': 'NumDaysInterest',
    '158': 'AccruedInterestRate',
    '159': 'AccruedInterestAmt',
    '160': 'SettlInstMode',
    '161': 'AllocText',
    '162': 'SettlInstID',
    '163': 'SettlInstTransType',
    '164': 'EmailThreadID',
    '165': 'SettlInstSource',
    '166': 'SettlLocation',
    '167': 'SecurityType',
    '168': 'EffectiveTime',
    '169': 'StandInstDbType',
    '170': 'StandInstDbName',
    '171': 'StandInstDbID',
    '172': 'SettlDeliveryType',
    '173': 'SettlDepositoryCode',
    '174': 'SettlBrkrCode',
    '175': 'SettlInstCode',
    '176': 'SecuritySettlAgentName',
    '177': 'SecuritySettlAgentCode',
    '178': 'SecuritySettlAgentAcctNum',
    '179': 'SecuritySettlAgentAcctName',
    '180': 'SecuritySettlAgentContactName',
    '181': 'SecuritySettlAgentContactPhone',
    '182': 'CashSettlAgentName',
    '183': 'CashSettlAgentCode',
    '184': 'CashSettlAgentAcctNum',
    '185': 'CashSettlAgentAcctName',
    '186': 'CashSettlAgentContactName',
    '187': 'CashSettlAgentContactPhone',
    '188': 'BidSpotRate',
    '189': 'BidForwardPoints',
    '190': 'OfferSpotRate',
    '191': 'OfferForwardPoints',
    '192': 'OrderQty2',
    '193': 'SettlDate2',
    '194': 'LastSpotRate',
    '195': 'LastForwardPoints',
    '196': 'AllocLinkID',
    '197': 'AllocLinkType',
    '198': 'SecondaryOrderID',
    '199': 'NoIOIQualifiers',
    '200': 'MaturityMonthYear',
    '201': 'PutOrCall',
    '202': 'StrikePrice',
    '203': 'CoveredOrUncovered',
    '204': 'CustomerOrFirm',
    '205': 'MaturityDay',
    '206': 'OptAttribute',
    '207': 'SecurityExchange',
    '208': 'NotifyBrokerOfCredit',
    '209': 'AllocHandlInst',
    '210': 'MaxShow',
    '211': 'PegOffsetValue',
    '212': 'XmlDataLen',
    '213': 'XmlData',
    '214': 'SettlInstRefID',
    '215': 'NoRoutingIDs',
    '216': 'RoutingType',
    '217': 'RoutingID',
    '218': 'Spread',
    '219': 'Benchmark',
    '220': 'BenchmarkCurveCurrency',
    '221': 'BenchmarkCurveName',
    '222': 'BenchmarkCurvePoint',
    '223': 'CouponRate',
    '224': 'CouponPaymentDate',
    '225': 'IssueDate',
    '226': 'RepurchaseTerm',
    '227': 'RepurchaseRate',
    '228': 'Factor',
    '229': 'TradeOriginationDate',
    '230': 'ExDate',
    '231': 'ContractMultiplier',
    '232': 'NoStipulations',
    '233': 'StipulationType',
    '234': 'StipulationValue',
    '235': 'YieldType',
    '236': 'Yield',
    '237': 'TotalTakedown',
    '238': 'Concession',
    '239': 'RepoCollateralSecurityType',
    '240': 'RedemptionDate',
    '241': 'UnderlyingCouponPaymentDate',
    '242': 'UnderlyingIssueDate',
    '243': 'UnderlyingRepoCollateralSecurityType',
    '244': 'UnderlyingRepurchaseTerm',
    '245': 'UnderlyingRepurchaseRate',
    '246': 'UnderlyingFactor',
    '247': 'UnderlyingRedemptionDate',
    '248': 'LegCouponPaymentDate',
    '249': 'LegIssueDate',
    '250': 'LegRepoCollateralSecurityType',
    '251': 'LegRepurchaseTerm',
    '252': 'LegRepurchaseRate',
    '253': 'LegFactor',
    '254': 'LegRedemptionDate',
    '255': 'CreditRating',
    '256': 'UnderlyingCreditRating',
    '257': 'LegCreditRating',
    '258': 'TradedFlatSwitch',
    '259': 'BasisFeatureDate',
    '260': 'BasisFeaturePrice',
    '262': 'MDReqID',
    '263': 'SubscriptionRequestType',
    '264': 'MarketDepth',
    '265': 'MDUpdateType',
    '266': 'AggregatedBook',
    '267': 'NoMDEntryTypes',
    '268': 'NoMDEntries',
    '269': 'MDEntryType',
    '270': 'MDEntryPx',
    '271': 'MDEntrySize',
    '272': 'MDEntryDate',
    '273': 'MDEntryTime',
    '274': 'TickDirection',
    '275': 'MDMkt',
    '276': 'QuoteCondition',
    '277': 'TradeCondition',
    '278': 'MDEntryID',
    '279': 'MDUpdateAction',
    '280': 'MDEntryRefID',
    '281': 'MDReqRejReason',
    '282': 'MDEntryOriginator',
    '283': 'LocationID',
    '284': 'DeskID',
    '285': 'DeleteReason',
    '286': 'OpenCloseSettlFlag',
    '287': 'SellerDays',
    '288': 'MDEntryBuyer',
    '289': 'MDEntrySeller',
    '290': 'MDEntryPositionNo',
    '291': 'FinancialStatus',
    '292': 'CorporateAction',
    '293': 'DefBidSize',
    '294': 'DefOfferSize',
    '295': 'NoQuoteEntries',
    '296': 'NoQuoteSets',
    '297': 'QuoteStatus',
    '298': 'QuoteCancelType',
    '299': 'QuoteEntryID',
    '300': 'QuoteRejectReason',
    '301': 'QuoteResponseLevel',
    '302': 'QuoteSetID',
    '303': 'QuoteRequestType',
    '304': 'TotNoQuoteEntries',
    '305': 'UnderlyingSecurityIDSource',
    '306': 'UnderlyingIssuer',
    '307': 'UnderlyingSecurityDesc',
    '308': 'UnderlyingSecurityExchange',
    '309': 'UnderlyingSecurityID',
    '310': 'UnderlyingSecurityType',
    '311': 'UnderlyingSymbol',
    '312': 'UnderlyingSymbolSfx',
    '313': 'MarginRatio',
    '314': 'MarginExcess',
    '315': 'TotalNetValue',
    '316': 'CashOutstanding',
    '317': 'CollAsgnID',
    '318': 'CollAsgnTransType',
    '319': 'CollAsgnRefID',
    '320': 'CollAsgnReason',
    '321': 'CollAsgnRejectReason',
    '322': 'CollAsgnRespType',
    '323': 'TrdRegTimestamp',
    '324': 'TrdRegTimestampType',
    '325': 'TrdRegTimestampOrigin',
    '326': 'ConfirmID',
    '327': 'ConfirmStatus',
    '328': 'ConfirmTransType',
    '329': 'ContractSettlMonth',
    '330': 'DeliveryForm',
    '331': 'LastParPx',
    '332': 'NoLinesOfText',
    '333': 'MsgSeqNum',
    '334': 'DayBookingInst',
    '335': 'BookingUnit',
    '336': 'MaturityMonthYear',
    '337': 'MaxShow',
    '338': 'TestMessageIndicator',
    '339': 'Username',
    '340': 'Password',
    '341': 'EncodedIssuerLen',
    '342': 'EncodedIssuer',
    '343': 'EncodedSecurityDescLen',
    '344': 'EncodedSecurityDesc',
    '345': 'EncodedListExecInstLen',
    '346': 'EncodedListExecInst',
    '347': 'EncodedTextLen',
    '348': 'EncodedText',
    '349': 'EncryptedPasswordMethod',
    '350': 'EncryptedPasswordLen',
    '351': 'EncryptedPassword',
    '352': 'EncryptedNewPasswordLen',
    '353': 'EncryptedNewPassword',
    '354': 'EncodedUnderlyingIssuerLen',
    '355': 'EncodedUnderlyingIssuer',
    '356': 'EncodedUnderlyingSecurityDescLen',
    '357': 'EncodedUnderlyingSecurityDesc',
    '358': 'AllocHandlInst',
    '359': 'MaxMessageSize',
    '360': 'NoMsgTypes',
    '361': 'MsgDirection',
    '362': 'NoTradingSessions',
    '363': 'TotalVolumeTraded',
    '364': 'DiscretionInst',
    '365': 'DiscretionOffsetValue',
    '366': 'BidID',
    '367': 'ClientBidID',
    '368': 'ListName',
    '369': 'TotalNumSecurities',
    '370': 'BrokerOfCredit',
    '371': 'UnderlyingCouponRate',
    '372': 'TotalVolumeTradedDate',
    '373': 'TotalVolumeTradedTime',
    '374': 'RejectText',
    '375': 'RefTagID',
    '376': 'RefMsgType',
    '377': 'SessionRejectReason',
    '378': 'BidRequestTransType',
    '379': 'ContraBroker',
    '380': 'ComplianceID',
    '381': 'SolicitedFlag',
    '382': 'ExecRestatementReason',
    '383': 'BusinessRejectRefID',
    '384': 'BusinessRejectReason',
    '385': 'GrossTradeAmt',
    '386': 'NoContraBrokers',
    '387': 'MaxMessageSize',
    '388': 'XmlDataLen',
    '389': 'XmlData',
    '390': 'BidTradeType',
    '391': 'CardIssNum',
    '392': 'BaseListingExchange',
    '393': 'TotalValueTraded',
    '394': 'NoRelatedSym',
    '395': 'ContractYearMonth',
    '396': 'ListStatus',
    '397': 'ContractMultiplier',
    '398': 'ContraTradeQty',
    '399': 'ContraTradeTime',
    '400': 'ClearingFirm',
    '401': 'ClearingAccount',
    '402': 'LiquidityNumSecurities',
    '403': 'OnBehalfOfSendingTime',
    '404': 'CapPrice',
    '405': 'NoStrikeRules',
    '406': 'StrikeRuleID',
    '407': 'StartStrikePx',
    '408': 'EndStrikePx',
    '409': 'StrikeIncrement',
    '410': 'StrikeExponent',
    '411': 'MinLotSize',
    '412': 'NoQuoteSets',
    '413': 'QuoteSetID',
    '414': 'QuoteSetValidUntilTime',
    '415': 'QuoteSetDepth',
    '416': 'TotNoQuoteEntries',
    '417': 'NoQuoteEntries',
    '418': 'QuoteEntryID',
    '419': 'QuoteEntrySize',
    '420': 'QuoteEntryPrice',
    '421': 'NoOrders',
    '422': 'OrderID',
    '423': 'PriceType',
    '424': 'OrdType',
    '425': 'ValidUntilTime',
    '426': 'ExpireTime',
    '427': 'DKReason',
    '428': 'DeliverToCompID',
    '429': 'DeliverToSubID',
    '430': 'IOINaturalFlag',
    '431': 'QuoteReqID',
    '432': 'BidPx',
    '433': 'OfferPx',
    '434': 'BidSize',
    '435': 'OfferSize',
    '436': 'NoMiscFees',
    '437': 'MiscFeeAmt',
    '438': 'MiscFeeCurr',
    '439': 'MiscFeeType',
    '440': 'PrevClosePx',
    '441': 'ResetSeqNumFlag',
    '442': 'SenderLocationID',
    '443': 'TargetLocationID',
    '444': 'OnBehalfOfLocationID',
    '445': 'DeliverToLocationID',
    '446': 'NoSecurityTypes',
    '447': 'SecurityType',
    '448': 'BasketRefID',
    '449': 'PutOrCall',
    '450': 'StrikePrice',
    '451': 'OptAttribute',
    '452': 'ContractMultiplier',
    '453': 'MDEntryID',
    '454': 'MDUpdateAction',
    '455': 'MDEntryRefID',
    '456': 'MDReqRejReason',
    '457': 'MDEntryOriginator',
    '458': 'LocationID',
    '459': 'DeskID',
    '460': 'DeleteReason'
};

// Also add common value mappings for specific fields
export const FIX_FIELD_VALUES: Record<string, Record<string, string>> = {
    '39': { // OrdStatus
        '0': 'New',
        '1': 'Partially Filled',
        '2': 'Filled',
        '3': 'Done for Day',
        '4': 'Canceled',
        '5': 'Replaced',
        '8': 'Rejected',
        'C': 'Expired'
    },
    '150': { // ExecType
        '0': 'New',
        '1': 'Partial fill',
        '2': 'Fill',
        '3': 'Done for day',
        '4': 'Canceled',
        '5': 'Replace',
        '6': 'Pending Cancel',
        '7': 'Stopped',
        '8': 'Rejected',
        '9': 'Suspended',
        'A': 'Pending New',
        'B': 'Calculated',
        'C': 'Expired',
        'D': 'Restated',
        'E': 'Pending Replace',
        'F': 'Trade',
        'G': 'Trade Correct',
        'H': 'Trade Cancel',
        'I': 'Order Status'
    },
    '54': { // Side
        '1': 'Buy',
        '2': 'Sell',
        '3': 'Buy Minus',
        '4': 'Sell Plus',
        '5': 'Sell Short',
        '6': 'Sell Short Exempt'
    },
    '40': { // OrdType
        '1': 'Market',
        '2': 'Limit',
        '3': 'Stop',
        '4': 'Stop Limit',
        '5': 'Market on Close',
        '6': 'With or Without',
        '7': 'Limit or Better',
        '8': 'Limit with or Without',
        '9': 'On Basis',
        'A': 'On Close',
        'B': 'Limit On Close',
        'C': 'Forex Market',
        'D': 'Previously Quoted',
        'E': 'Previously Indicated',
        'F': 'Forex Limit',
        'G': 'Forex Swap',
        'H': 'Forex Previously Quoted'
    }
};

export const getFieldDescription = (tag: string, value: string): string => {
    const tagName = FIX_TAGS[tag] || `Tag ${tag}`;
    const valueMap = FIX_FIELD_VALUES[tag];
    if (valueMap) {
        const valueDesc = valueMap[value];
        return `${tagName}: ${value}${valueDesc ? ` (${valueDesc})` : ''}`;
    }
    return `${tagName}: ${value}`;
};
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

namespace Application.Accounting.Services;

/// <summary>
/// Represents the result of a tax service operation, mimicking OFBiz’s ServiceUtil.
/// Contains success status, data, or error message for tax-related calculations.
/// </summary>
public class TaxServiceResult
{
    // Indicates whether the operation was successful.
    public bool Success { get; }
    // Contains the result data if successful, or null if an error occurred.
    public object Data { get; }
    // Contains the error message if the operation failed, or null if successful.
    public string ErrorMessage { get; }

    // Initializes a new TaxServiceResult with the specified values.
    private TaxServiceResult(bool success, object data, string errorMessage)
    {
        Success = success;
        Data = data;
        ErrorMessage = errorMessage;
    }

    // Creates a successful result with optional data.
    public static TaxServiceResult CreateSuccess(object data = null) => new TaxServiceResult(true, data, null);
    // Creates an error result with a message.
    public static TaxServiceResult Error(string errorMessage) => new TaxServiceResult(false, null, errorMessage);
}
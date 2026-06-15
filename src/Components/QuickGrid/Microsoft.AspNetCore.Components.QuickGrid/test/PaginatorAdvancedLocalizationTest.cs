// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.QuickGrid.Infrastructure;
using Xunit;

namespace Microsoft.AspNetCore.Components.QuickGrid.Tests;

public class PaginatorAdvancedLocalizationTest
{
        private sealed class TestLocalizer : IPaginatorLocalizer2
        {
            public string FirstPageAriaLabel => "First";
            public string PreviousPageAriaLabel => "Prev";
            public string NextPageAriaLabel => "Next";
            public string LastPageAriaLabel => "Last";
            public string FirstPageTitle => "First";
            public string PreviousPageTitle => "Prev";
            public string NextPageTitle => "Next";
            public string LastPageTitle => "Last";
            public string ItemSingularText => "item";
            public string ItemPluralText => "items";
            public string PageLabelFormat => "Page {0} of {1}";
            public string Items(int count) => count == 1 ? "ENTRY" : "ENTRIES";
            public string PageLabel(int currentPage, int totalPages) => $"P{currentPage}/{totalPages}";
            public string Summary(PaginationState state)
            {
                var count = state.TotalItemCount ?? 0;
                return $"S:{count}";
            }
        }

        [Fact]
        public async Task PageLabel_UsesParameterFormatter_OverLocalizer()
        {
            var state = new PaginationState { ItemsPerPage = 10 };
            // Set total items via non-public API
            var setTotal = typeof(PaginationState).GetMethod("SetTotalItemCountAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
            await (Task)setTotal.Invoke(state, new object[] { 42 })!;
            await state.SetCurrentPageIndexAsync(1); // page 2
            var paginator = new Paginator
            {
                State = state,
                Localizer = new TestLocalizer(),
                PageLabelFormatter = (c, t) => $"C{c}-T{t}"
            };

            var method = typeof(Paginator).GetMethod("get_ResolvedPageLabelText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = (string)method!.Invoke(paginator, null)!;
            Assert.Equal("C2-T5", value);
        }

        [Fact]
        public async Task Items_UsesLocalizer2_WhenAvailable()
        {
            var state = new PaginationState();
            var setTotal = typeof(PaginationState).GetMethod("SetTotalItemCountAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
            await (Task)setTotal.Invoke(state, new object[] { 2 })!;
            var paginator = new Paginator
            {
                State = state,
                Localizer = new TestLocalizer(),
            };

            var method = typeof(Paginator).GetMethod("get_ResolvedItemsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = (string)method!.Invoke(paginator, null)!;
            Assert.Equal("ENTRIES", value);
        }

        [Fact]
        public async Task Summary_UsesSummaryFormatter_OverLocalizer()
        {
            var state = new PaginationState();
            var setTotal = typeof(PaginationState).GetMethod("SetTotalItemCountAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
            await (Task)setTotal.Invoke(state, new object[] { 7 })!;
            var paginator = new Paginator
            {
                State = state,
                Localizer = new TestLocalizer(),
                SummaryFormatter = s => $"SUM:{s.TotalItemCount}"
            };

            var method = typeof(Paginator).GetMethod("get_ResolvedSummaryText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var value = (string)method!.Invoke(paginator, null)!;
            Assert.Equal("SUM:7", value);
        }
    }

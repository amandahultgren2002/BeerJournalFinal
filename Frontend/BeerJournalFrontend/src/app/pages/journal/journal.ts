// Journal component — shows the list of all tasting entries for the logged-in user
// Includes stats at the top (total beers, average rating, most common style)

import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TastingEntryService } from '../../services/tasting-entry-service';
import { TastingEntry } from '../../models/tasting-entry';

@Component({
  selector: 'app-journal',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './journal.html',
  styleUrl: './journal.css'
})
export class Journal implements OnInit {

  // Holds all the user's tasting entries — starts empty, filled in ngOnInit()
  entries: TastingEntry[] = [];

  // Constructor — Angular gives us the services we asked for (dependency injection)
  constructor(
    private entryService: TastingEntryService,
    private cdr: ChangeDetectorRef
  ) {}

  // Runs automatically when the component loads
  // Fetches all entries from the backend
  ngOnInit() {
    this.entryService.getEntries().subscribe({
      next: (data) => {
        this.entries = data;
        this.cdr.markForCheck();   // tell Angular to refresh the page
      },
      error: (err) => console.error('Error loading entries', err)
    });
  }

  // Calculates the average rating of all entries
  // The "get" keyword makes this a getter — we use it like a property in the HTML: {{ avgRating }}
  get avgRating(): string {
    if (this.entries.length === 0) return '—';

    // Add up all the ratings (treat null as 0)
    const sum = this.entries.reduce((acc, e) => acc + (e.rating ?? 0), 0);

    // Divide by the number of entries
    const avg = sum / this.entries.length;

    // toFixed(1) keeps one decimal place — e.g. 7.5 instead of 7.4999
    return avg.toFixed(1);
  }

  // Finds which beer category appears the most in the user's entries
  get topStyle(): string {
    if (this.entries.length === 0) return '—';

    // Build a tally of categories — e.g. { "IPA": 3, "Pilsner": 2 }
    const counts: Record<string, number> = {};

    this.entries.forEach(e => {
      // The "?." is optional chaining — safe to use even if beer is missing
      const cat = e.beer?.category;
      if (cat) counts[cat] = (counts[cat] || 0) + 1;
    });

    // Sort by count (highest first) and return the top one
    const sorted = Object.entries(counts).sort((a, b) => b[1] - a[1]);
    return sorted[0]?.[0] ?? '—';
  }

  // Deletes an entry — called when the user clicks the 🗑 button
  deleteEntry(id: number) {
    this.entryService.deleteEntry(id).subscribe({
      next: () => {
        // Remove the deleted entry from the local list
        // (so we don't have to refetch everything from the backend)
        this.entries = this.entries.filter(e => e.entryId !== id);
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Error deleting entry', err)
    });
  }
}
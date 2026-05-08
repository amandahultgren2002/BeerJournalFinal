// Log-beer component — form for creating a new tasting entry or editing an existing one
// Same form is used for both, switching mode based on whether there's an :id in the URL

import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { TastingEntryService } from '../../services/tasting-entry-service';
import { BeerService } from '../../services/beer-service';
import { Beer } from '../../models/beer';

@Component({
  selector: 'app-log-beer',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './log-beer.html',
  styleUrl: './log-beer.css'
})
export class LogBeer implements OnInit {

  // List of all beers from the database (for the dropdown)
  beers: Beer[] = [];

  // Which beer the user picked: a beer id, "new" (add a new one), or null
  selectedBeerId: number | 'new' | null = null;

  // Fields used only when adding a new beer
  newBeerName = '';
  newBeerBrand = '';
  newBeerAlcoholPct = '';
  newBeerCategory = '';

  // Tasting entry fields (filled from the form)
  price = '';
  location = '';
  rating = 0;
  notes = '';
  latitude: number | null = null;
  longitude: number | null = null;

  // Date of the tasting — set automatically (today for new entries, kept as-is for edits)
  testingDate: string | null = null;

  // UI state
  error: string | null = null;
  submitting = false;

  // Edit mode — null when creating, has the id when editing
  editingId: number | null = null;

  // Beer style options for the "add new beer" pills
  categories = ['Pilsner', 'IPA', 'Stout', 'Wheat', 'Sour', 'Lager', 'Porter', 'Other'];

  constructor(
    private entryService: TastingEntryService,
    private beerService: BeerService,
    private router: Router,
    private route: ActivatedRoute,        // used to read the :id from the URL
    private cdr: ChangeDetectorRef
  ) {}

  // Runs when the component loads
  ngOnInit() {
    // Load the beer catalogue (always needed)
    this.beerService.getBeers().subscribe({
      next: (data) => {
        this.beers = data;
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Error loading beers', err)
    });

    // If there's an :id in the URL we're in edit mode — load that entry
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.editingId = parseInt(idParam, 10);
      this.loadEntryForEditing(this.editingId);
    }
  }

  // Fetch one entry from the backend and pre-fill the form with its values
  private loadEntryForEditing(id: number) {
    this.entryService.getEntry(id).subscribe({
      next: (entry) => {
        this.selectedBeerId = entry.beerId;
        this.rating         = entry.rating ?? 0;
        this.location       = entry.location ?? '';
        this.notes          = entry.notes ?? '';
        this.latitude       = entry.latitude;
        this.longitude      = entry.longitude;
        this.testingDate    = entry.testingDate;

        // Price is a number in the database but a string in the form
        this.price = entry.price !== null && entry.price !== undefined
          ? entry.price.toString()
          : '';

        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = 'Could not load entry for editing';
        console.error(err);
      }
    });
  }

  // True when the user picked "+ Add new beer" — used to show the new-beer fields
  get isAddingNewBeer(): boolean {
    return this.selectedBeerId === 'new';
  }

  // True when we're editing an existing entry — used for the page title and button text
  get isEditing(): boolean {
    return this.editingId !== null;
  }

  // Star click — sets the rating
  setRating(value: number) {
    this.rating = value;
  }

  // Pill click — sets the category for a new beer
  setCategory(cat: string) {
    this.newBeerCategory = cat;
  }

  // Looks up GPS coordinates from the location text using OpenStreetMap
  lookupLocation() {
    if (!this.location) return;

    fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(this.location)}`)
      .then(res => res.json())
      .then(results => {
        if (results.length > 0) {
          this.latitude = parseFloat(results[0].lat);
          this.longitude = parseFloat(results[0].lon);
        }
      });
  }

  // Submit handler — runs when the user clicks "Log Beer" or "Save Changes"
  onSubmit() {
    // Validate
    if (this.selectedBeerId === null) {
      this.error = 'Please select a beer or add a new one';
      return;
    }
    if (this.rating === 0) {
      this.error = 'Please give a rating';
      return;
    }
    if (this.isAddingNewBeer && !this.newBeerName) {
      this.error = 'Please enter a name for the new beer';
      return;
    }

    this.submitting = true;
    this.error = null;

    // Branch 1: adding a new beer — create the beer first, then save the entry
    if (this.isAddingNewBeer) {
      this.beerService.createBeer({
        name:       this.newBeerName,
        brand:      this.newBeerBrand      || null,
        alcoholPct: this.newBeerAlcoholPct ? parseFloat(this.newBeerAlcoholPct) : null,
        category:   this.newBeerCategory   || null
      }).subscribe({
        next: (createdBeer) => this.saveEntry(createdBeer.beerId),
        error: (err) => this.handleError(err)
      });
    }
    // Branch 2: existing beer was picked
    else {
      this.saveEntry(this.selectedBeerId as number);
    }
  }

  // Saves the entry — POST for new, PUT for edit
  private saveEntry(beerId: number) {
    // New entries get today's date, edits keep their original date
    // toISOString() gives "2026-05-08T12:34:56.789Z" — slice(0, 10) keeps just "2026-05-08"
    const dateToSend = this.isEditing
      ? this.testingDate
      : new Date().toISOString().slice(0, 10);

    // Data object — same shape for create and update
    const entryData = {
      beerId:      beerId,
      rating:      this.rating,
      location:    this.location || null,
      price:       this.price ? parseFloat(this.price) : null,
      notes:       this.notes || null,
      testingDate: dateToSend,
      latitude:    this.latitude,
      longitude:   this.longitude
    };

    // Choose POST or PUT based on edit mode
    const request$ = this.isEditing
      ? this.entryService.updateEntry(this.editingId!, entryData)   // PUT
      : this.entryService.createEntry(entryData);                   // POST

    request$.subscribe({
      next: () => this.router.navigate(['/journal']),
      error: (err) => this.handleError(err)
    });
  }

  // Shared error handler
  private handleError(err: any) {
    this.error = 'Error ' + err.status;
    this.submitting = false;
    this.cdr.markForCheck();
  }
}
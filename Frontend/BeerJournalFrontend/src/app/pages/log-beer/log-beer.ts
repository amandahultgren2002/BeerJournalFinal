// Imports — Angular tools, routing, our services, and our models
import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
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

  // ── Beer dropdown state ─────────────────────────────────
  // Holds the catalogue we fetch from the backend
  beers: Beer[] = [];

  // Which beer the user picked. Either a beer's id, the special
  // string "new" (meaning "add a new beer"), or null (nothing picked yet)
  selectedBeerId: number | 'new' | null = null;

  // ── Fields used only when adding a NEW beer ────────────
  newBeerName = '';
  newBeerBrand = '';
  newBeerAlcoholPct = '';
  newBeerCategory = '';

  // ── Tasting entry fields (the rating, location, etc.) ──
  price = '';
  location = '';
  rating = 0;
  notes = '';
  latitude: number | null = null;
  longitude: number | null = null;

  // ── UI state ────────────────────────────────────────────
  error: string | null = null;
  submitting = false;

  // List of beer styles for the "add new beer" category buttons
  categories = ['Pilsner', 'IPA', 'Stout', 'Wheat', 'Sour', 'Lager', 'Porter', 'Other'];

  constructor(
    private entryService: TastingEntryService,
    private beerService: BeerService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  // Runs when the component loads — fetch the beer catalogue
  ngOnInit() {
    this.beerService.getBeers().subscribe({
      next: (data) => {
        this.beers = data;
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Error loading beers', err)
    });
  }

  // Helper for the template: are we currently adding a new beer?
  // Used to show/hide the "new beer" fields
  get isAddingNewBeer(): boolean {
    return this.selectedBeerId === 'new';
  }

  // Click handler for the rating stars
  setRating(value: number) {
    this.rating = value;
  }

  // Click handler for the category buttons (only used when adding a new beer)
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

  // Main submit handler. The flow is:
  //   1. Validate the form
  //   2. If user is adding a new beer → create it first, get its id
  //   3. Create the tasting entry with that beer id
  onSubmit() {
    // Basic validation
    if (this.selectedBeerId === null) {
      this.error = 'Please select a beer or add a new one';
      return;
    }
    if (this.rating === 0) {
      this.error = 'Please give a rating';
      return;
    }
    // Extra validation when adding a new beer
    if (this.isAddingNewBeer && !this.newBeerName) {
      this.error = 'Please enter a name for the new beer';
      return;
    }

    this.submitting = true;
    this.error = null;

    // Branch 1: user is adding a NEW beer to the catalogue
    if (this.isAddingNewBeer) {
      this.beerService.createBeer({
        name:       this.newBeerName,
        brand:      this.newBeerBrand      || null,
        alcoholPct: this.newBeerAlcoholPct ? parseFloat(this.newBeerAlcoholPct) : null,
        category:   this.newBeerCategory   || null
      }).subscribe({
        next: (createdBeer) => {
          // Beer was created — now create the entry using its new id
          this.createEntry(createdBeer.beerId);
        },
        error: (err) => this.handleError(err)
      });
    }
    // Branch 2: user picked an existing beer
    else {
      // selectedBeerId is a number here (we already excluded null and 'new')
      this.createEntry(this.selectedBeerId as number);
    }
  }

  // Creates the actual tasting entry once we have a beerId
  private createEntry(beerId: number) {
    this.entryService.createEntry({
      beerId:    beerId,
      rating:    this.rating,
      location:  this.location || null,
      price:     this.price ? parseFloat(this.price) : null,
      notes:     this.notes || null,
      latitude:  this.latitude,
      longitude: this.longitude
    }).subscribe({
      next: () => this.router.navigate(['/journal']),
      error: (err) => this.handleError(err)
    });
  }

  // Shared error handler used by both submit branches
  private handleError(err: any) {
    this.error = 'Error ' + err.status;
    this.submitting = false;
    this.cdr.markForCheck();
  }
}
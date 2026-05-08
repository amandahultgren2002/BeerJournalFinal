// Map component — shows all the user's tasting entries as pins on a map
// Uses the Leaflet library (open source mapping) and OpenStreetMap tiles

import { Component, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';
import { TastingEntryService } from '../../services/tasting-entry-service';
import { TastingEntry } from '../../models/tasting-entry';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map.html',
  styleUrl: './map.css'
})
export class Map implements AfterViewInit {

  // Holds the entries we'll show as markers
  entries: TastingEntry[] = [];

  constructor(
    private entryService: TastingEntryService,
    private cdr: ChangeDetectorRef
  ) {}

  // Runs after the HTML is rendered (so the #beer-map div exists)
  // We need ngAfterViewInit instead of ngOnInit because Leaflet needs
  // a real <div> in the DOM to attach the map to
  ngAfterViewInit(): void {
    this.loadEntries();
  }

  // Fetch entries from the backend, then draw the map
  loadEntries() {
    this.entryService.getEntries().subscribe({
      next: (data) => {
        this.entries = data;
        this.initMap();
        this.cdr.markForCheck();
      },
      error: (err) => console.error('Error loading entries', err)
    });
  }

  // Build the Leaflet map and drop a marker for each entry with coordinates
  initMap() {
    // Center the map on Copenhagen, zoom level 14
    const map = L.map('beer-map').setView([55.6761, 12.5683], 14);

    // Add the OpenStreetMap tile layer (the actual map background)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19
    }).addTo(map);

    // Custom marker icon — a big 📍 emoji
    const beerIcon = L.divIcon({
      html: '<span style="font-size: 70px;">📍</span>',
      iconSize: [48, 48],
      className: ''
    });

    // Loop through entries and place a marker for each one with coordinates
    this.entries.forEach(entry => {
      if (entry.latitude && entry.longitude) {
        const marker = L.marker(
          [entry.latitude, entry.longitude],
          { icon: beerIcon }
        ).addTo(map);

        // Popup shown when the user clicks the marker
        // Beer name and brand come from the nested beer object
        marker.bindPopup(`
          <b>🍺 ${entry.beer?.name ?? ''}</b><br>
          ${entry.beer?.brand ? entry.beer.brand + '<br>' : ''}
          ★ ${entry.rating}/10<br>
          📍 ${entry.location}
        `);
      }
    });
  }
}
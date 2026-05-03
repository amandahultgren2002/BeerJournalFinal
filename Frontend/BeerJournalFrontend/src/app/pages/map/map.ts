// Imports — Angular tools, Leaflet for the map, and our typed entry service
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

  // Holds the entries we'll show as markers — properly typed now
  entries: TastingEntry[] = [];

  constructor(
    private entryService: TastingEntryService,
    private cdr: ChangeDetectorRef
  ) {}

  // Runs after the HTML is rendered (so the #beer-map div exists)
  ngAfterViewInit(): void {
    this.loadEntries();
  }

  // Fetches entries from the backend, then draws the map
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

  // Builds the Leaflet map and drops a marker for each entry that has coordinates
  initMap() {
    // Center the map on Copenhagen, zoom level 14
    const map = L.map('beer-map').setView([55.6761, 12.5683], 14);

    // Add the OpenStreetMap tile layer (the actual map background)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19
    }).addTo(map);

    // A custom marker icon — a big 📍 emoji
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

        // The popup that shows when you click the marker
        // Beer name and brand now come from the nested beer object
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
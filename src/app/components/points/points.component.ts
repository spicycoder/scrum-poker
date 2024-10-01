import { Component, computed, input } from '@angular/core';
import CardDetails from '../../models/CardDetails';
import { PokerCardComponent } from '../poker-card/poker-card.component';

@Component({
  selector: 'app-points',
  standalone: true,
  imports: [PokerCardComponent],
  templateUrl: './points.component.html',
  styleUrl: './points.component.css'
})
export class PointsComponent {
  points = input.required<CardDetails[]>();

  transformedPoints = computed<CardDetails[]>(() =>
    this.points().map(point => ({
      ...point,
      display: point.value,
      value: point.value ? this.points().filter(p => p.value === point.value).length.toString() : null
    }))
  );

  show = computed(() => this.transformedPoints().every(point => point.value !== null));
}

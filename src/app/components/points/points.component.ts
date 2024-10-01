import { Component, computed, input } from '@angular/core';
import CardDetails from '../../models/CardDetails';
import { PokerCardComponent } from '../poker-card/poker-card.component';

@Component({
  selector: 'app-points',
  standalone: true,
  imports: [PokerCardComponent],
  templateUrl: './points.component.html',
  styleUrl: './points.component.css',
})
export class PointsComponent {
  points = input.required<CardDetails[]>();
  transformedPoints = computed<CardDetails[]>(() =>
    Array.from(
      this.points()
        .reduce(
          (map, point) =>
            point.value && !map.has(point.value)
              ? map.set(point.value, {
                  ...point,
                  display: point.value,
                  value: this.points()
                    .filter((p) => p.value === point.value)
                    .length.toString(),
                })
              : map,
          new Map()
        )
        .values()
    )
  );

  show = computed(() =>
    this.transformedPoints().every((point) => point.value !== null)
  );
}

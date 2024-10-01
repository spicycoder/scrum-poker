import { NgClass } from '@angular/common';
import { Component, input, Input } from '@angular/core';
import CardDetails from '../../models/CardDetails';

@Component({
  selector: 'app-poker-card',
  standalone: true,
  imports: [NgClass],
  templateUrl: './poker-card.component.html',
  styleUrl: './poker-card.component.css'
})
export class PokerCardComponent {
  card = input.required<CardDetails>();
  show = input<boolean>(true);
}

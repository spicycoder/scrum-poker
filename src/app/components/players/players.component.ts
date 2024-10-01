import { Component, computed, input, Input } from '@angular/core';
import CardDetails from '../../models/CardDetails';
import { PokerCardComponent } from "../poker-card/poker-card.component";

@Component({
  selector: 'app-players',
  standalone: true,
  imports: [PokerCardComponent],
  templateUrl: './players.component.html',
  styleUrl: './players.component.css'
})
export class PlayersComponent {
  players = input.required<CardDetails[]>();
  show = computed(() => this.players().every(player => player.value !== null));
}

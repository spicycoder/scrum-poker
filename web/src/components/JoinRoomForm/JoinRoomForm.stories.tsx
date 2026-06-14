import type { Meta, StoryObj } from '@storybook/react';
import { MemoryRouter } from 'react-router-dom';
import JoinRoomForm from './JoinRoomForm';

const meta: Meta<typeof JoinRoomForm> = {
  title: 'Tabs/Join',
  component: JoinRoomForm,
  tags: ['autodocs'],
  decorators: [
    (Story) => (
      <MemoryRouter>
        <div className="w-full max-w-md mx-auto mt-8">
          <Story />
        </div>
      </MemoryRouter>
    ),
  ],
};

export default meta;
type Story = StoryObj<typeof JoinRoomForm>;

export const Default: Story = {};

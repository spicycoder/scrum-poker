import { Tabs, TabsContent, TabsList, TabsTrigger } from '../ui/tabs'
import CreateRoomForm from '../CreateRoomForm'
import JoinRoomForm from '../JoinRoomForm'

interface HomePageProps {
  defaultTab?: 'create' | 'join'
  roomId?: string
}

export default function HomePage({ defaultTab = 'create', roomId }: HomePageProps) {
  return (
    <div className="flex justify-center px-6 pt-8">
      <Tabs defaultValue={defaultTab} className="w-full max-w-md">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="create">Create Room</TabsTrigger>
          <TabsTrigger value="join">Join Room</TabsTrigger>
        </TabsList>
        <TabsContent value="create">
          <CreateRoomForm />
        </TabsContent>
        <TabsContent value="join">
          <JoinRoomForm roomId={roomId} />
        </TabsContent>
      </Tabs>
    </div>
  )
}
